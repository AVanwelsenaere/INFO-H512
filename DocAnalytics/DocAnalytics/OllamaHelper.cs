using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;

namespace DocAnalytics
{
    public static class OllamaHelper
    {
        private static readonly HttpClient client = new HttpClient();

        public const int _iDefaultChunkSize = 200;
        public const int _iDefaultTimeoutSeconds = 1200;                    // Timeout SQL
        public const string _sEmbeddingsModel = "nomic-embed-text";         // "snowflake-arctic-embed2"

        private static readonly string apiUrl = "http://localhost:11434/api/embed"; // URL Ollama

        public static async Task<float[]> GenerateQueryEmbedding(string query)
        {
            return await ComputeEmbeddingOllama(query);
        }

        public static async Task<float[]> ComputeEmbeddingOllama(string text)
        {
            try
            {
                var requestBody = new
                {
                    model = _sEmbeddingsModel,  // Embeddings Model
                    input = new[] { text } 
                };

                string json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                string responseString = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Answer API Ollama : {responseString}"); // Debug

                return ExtractEmbeddingFromJson(responseString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating embedding : {ex.Message}");
                return null;
            }
        }

        private static float[] ExtractEmbeddingFromJson(string json)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("embeddings", out JsonElement embeddingsArray) && embeddingsArray.GetArrayLength() > 0)
                {
                    JsonElement embeddingArray = embeddingsArray[0];

                    List<float> embeddingList = new List<float>();
                    foreach (JsonElement item in embeddingArray.EnumerateArray())
                    {
                        embeddingList.Add(item.GetSingle());
                    }
                    return embeddingList.ToArray();
                }
                else
                {
                    Console.WriteLine("No embedding found in the response.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting embedding : {ex.Message}");
                return null;
            }
        }

        private static string GetSpeciesSummaryForPage(int pageId, SqlConnection conn)
        {
            using (SqlCommand cmd = new SqlCommand(@"
        SELECT species_order, species_family, species_genus, species_species
        FROM Page_Species
        WHERE species_page_id_fkey = @page_id", conn))
            {
                cmd.Parameters.AddWithValue("@page_id", pageId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    List<string> lines = new List<string>();

                    while (reader.Read())
                    {
                        string order = reader["species_order"]?.ToString();
                        string family = reader["species_family"]?.ToString();
                        string genus = reader["species_genus"]?.ToString();
                        string species = reader["species_species"]?.ToString();

                        lines.Add($"{order} > {family} > {genus} {species}");
                    }

                    if (lines.Count > 0)
                    {
                        return string.Join("\n", lines);
                    }
                }
            }

            return null; 
        }


        public static async Task ComputeEmbeddingsForBook(int bookId, SqlConnection conn, IProgress<int> progressBar)
        {
            try
            {
                DeleteExistingEmbeddings(bookId, conn);

                List<(int, Dictionary<string, string>)> pages = GetPagesForBook(bookId, conn);
                int totalChunks = 0;
                List<(int, string, string)> chunksToProcess = new List<(int, string, string)>();
                int totalPages = pages.Count;
                int processedPages = 0;

                foreach (var (pageId, textFields) in pages)
                {
                    if (textFields.ContainsKey("LLM"))
                    {
                        List<string> chunks = SplitIntoChunks(textFields["LLM"]);
                        totalChunks += chunks.Count;

                        foreach (string chunk in chunks)
                        {
                            chunksToProcess.Add((pageId, "LLM", chunk));
                        }
                    }
                    if (textFields.ContainsKey("CleanLLM"))
                    {
                        List<string> chunks = SplitIntoChunks(textFields["CleanLLM"]);
                        totalChunks += chunks.Count;

                        foreach (string chunk in chunks)
                        {
                            chunksToProcess.Add((pageId, "CleanLLM", chunk));
                        }
                    }

                    // Taxonomy chunk
                    string speciesSummary = GetSpeciesSummaryForPage(pageId, conn);
                    if (!string.IsNullOrWhiteSpace(speciesSummary))
                    {
                        string label = "[TAXONOMY]";
                        string taxoChunk = $"{label}\n{speciesSummary}";
                        chunksToProcess.Add((pageId, "TAXO", taxoChunk));
                        totalChunks++;
                    }
                }

                // Processing embeddings for each chunk
                int processedChunks = 0;
                foreach (var (pageId, textType, chunk) in chunksToProcess)
                {
                    float[] embedding = await ComputeEmbeddingOllama(chunk);
                    if (embedding != null)
                    {
                        SaveEmbeddingToDatabase(conn, pageId, textType, chunk, embedding);
                    }

                    processedChunks++;
                    int progressPercent = (processedChunks * 100) / totalChunks;

                    progressBar.Report(progressPercent); // Update ProgressBar
                }

                MessageBox.Show("Success !");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error with embeddings computing: {ex.Message}");
            }
        }


        private static List<(int, Dictionary<string, string>)> GetPagesForBook(int bookId, SqlConnection conn)
        {
            List<(int, Dictionary<string, string>)> pages = new List<(int, Dictionary<string, string>)>();

            string query = @"
                SELECT page_id_pkey, page_raw_text, page_ocr_text, page_raw_text_cleaned, page_raw_text_llm, page_raw_text_llm_explain
                FROM Pages
                WHERE page_book_id_fkey = @BookId";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@BookId", bookId);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int pageId = reader.GetInt32(0);
                        Dictionary<string, string> textFields = new Dictionary<string, string>
                {
                    { "Raw", reader.GetString(1) },
                    { "OCR", reader.GetString(2) },
                    { "Cleaned", reader.GetString(3) },
                    { "LLM", reader.GetString(4) },
                    { "LLM_Explain", reader.GetString(5) }
                };

                        // Add an "All" field which is the concatenation of the other 5
                        textFields["All"] = string.Join(" ", textFields.Values);
                        textFields["CleanLLM"] = textFields["LLM"] + " " + textFields["Cleaned"];

                        pages.Add((pageId, textFields));
                    }
                }
            }

            return pages;
        }

        private static List<string> SplitIntoChunks(string text, int chunkSize = 200)
        {
            List<string> chunks = new List<string>();
            string[] words = text.Split(' ');

            for (int i = 0; i < words.Length; i += chunkSize)
            {
                string chunk = string.Join(" ", words.Skip(i).Take(chunkSize));
                chunks.Add(chunk);
            }

            return chunks;
        }


        private static void SaveEmbeddingToDatabase(SqlConnection conn, int pageId, string textType, string text, float[] embedding)
        {
            // Calculate the embedding norm
            double embeddingNorm = Math.Sqrt(embedding.Sum(value => value * value));

            // Insert the embedding with embedding_norm
            string query = @"
                INSERT INTO Embeddings 
                (embedding_page_id_fkey, embedding_text_type, embedding_text, embedding_value, embedding_norm) 
                OUTPUT INSERTED.embedding_id_pkey
                VALUES (@PageId, @TextType, @TextContent, @Embedding, @EmbeddingNorm)";

            int embeddingId;
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@PageId", pageId);
                cmd.Parameters.AddWithValue("@TextType", textType);
                cmd.Parameters.AddWithValue("@TextContent", text);
                cmd.Parameters.AddWithValue("@Embedding", Encoding.UTF8.GetBytes(JsonSerializer.Serialize(embedding)));
                cmd.Parameters.AddWithValue("@EmbeddingNorm", embeddingNorm);

                object result = cmd.ExecuteScalar();
                if (result == null)
                {
                    throw new Exception("Failed to retrieve embedding_id_pkey.");
                }

                embeddingId = (int)result;
            }

            // Insert each value of the embedding into Embeddings_Values
            string insertVectorQuery = "INSERT INTO Embeddings_Values (embval_embedding_id_fkey, embval_vector_index, embval_vector_value) VALUES (@EmbeddingId, @Index, @Value)";

            using (SqlCommand cmd = new SqlCommand(insertVectorQuery, conn))
            {
                cmd.Parameters.Add("@EmbeddingId", System.Data.SqlDbType.Int).Value = embeddingId;
                cmd.Parameters.Add("@Index", System.Data.SqlDbType.Int);
                cmd.Parameters.Add("@Value", System.Data.SqlDbType.Float);

                for (int i = 0; i < embedding.Length; i++)
                {
                    cmd.Parameters["@Index"].Value = i;
                    cmd.Parameters["@Value"].Value = embedding[i];
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public static List<(int, string, string, string, string, string, string, float, int)> SearchEmbeddings(SqlConnection conn, float[] queryEmbedding, int topN, string targetBook)
        {
            List<(int, string, string, string, string, string, string, float, int)> results = new List<(int, string, string, string, string, string, string, float, int)>();

            DataTable queryTable = new DataTable();
            queryTable.Columns.Add("vector_index", typeof(int));
            queryTable.Columns.Add("vector_value", typeof(float));

            for (int i = 0; i < queryEmbedding.Length; i++)
            {
                queryTable.Rows.Add(i, queryEmbedding[i]);
            }

            using (SqlCommand cmd = new SqlCommand("SearchSimilarPages", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 1200;  // 120 seconds

                SqlParameter param = cmd.Parameters.AddWithValue("@query_embedding", queryTable);
                param.SqlDbType = SqlDbType.Structured;
                cmd.Parameters.AddWithValue("@top_n", topN);
                cmd.Parameters.AddWithValue("@book_title", targetBook == "All Books" ? (object)DBNull.Value : targetBook);

                string sSpeciesInfo = "";

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int pageId = reader.GetInt32(0);

                        string bookName = reader.GetString(1);
                        string textType = reader.GetString(2);
                        string fullText = reader.GetString(3);
                        string cleanedText = reader.GetString(4);
                        string llmText = reader.GetString(5);
                        float score = (float)reader.GetDouble(6);
                        int page_number = (int)reader.GetInt32(7);

                        results.Add((pageId, bookName, textType, llmText, cleanedText, llmText, llmText, score, page_number));
                        //results.Add((pageId, bookName, textType, fullText, cleanedText, llmText, fullText, score, page_number));
                    }



                }
            }

            return results;
        }


        public static void DeleteExistingEmbeddings(int bookId, SqlConnection conn)
        {
            string deleteVectorsQuery = @"
                DELETE FROM Embeddings_Values 
                WHERE embval_embedding_id_fkey IN (
                    SELECT embedding_id_pkey 
                    FROM Embeddings 
                    WHERE embedding_page_id_fkey IN (
                        SELECT page_id_pkey FROM Pages WHERE page_book_id_fkey = @BookId
                    )
                )";

            using (SqlCommand cmd = new SqlCommand(deleteVectorsQuery, conn))
            {
                cmd.Parameters.AddWithValue("@BookId", bookId);
                cmd.ExecuteNonQuery();
            }

            string deleteEmbeddingsQuery = @"
                DELETE FROM Embeddings 
                WHERE embedding_page_id_fkey IN (
                    SELECT page_id_pkey FROM Pages WHERE page_book_id_fkey = @BookId
                )";

            using (SqlCommand cmd = new SqlCommand(deleteEmbeddingsQuery, conn))
            {
                cmd.Parameters.AddWithValue("@BookId", bookId);
                cmd.ExecuteNonQuery();
            }
        }


        public static async Task QueryOllamaMistralStreamed(
            string userQuery,
            List<(string, string)> context,
            string selectedModel,
            int maxTokens,
            string language,
            Action<string> onUpdate)
        {
            using HttpClient client = new HttpClient();
            string apiUrl = "http://localhost:11434/api/generate";

            StringBuilder contextBuilder = new StringBuilder();
            foreach (var (cleaned, llm) in context)
            {
                contextBuilder.AppendLine("[Page Context]");
                // contextBuilder.AppendLine($"Cleaned: {cleaned}");
                contextBuilder.AppendLine($"LLM: {llm}");
                contextBuilder.AppendLine();
            }

            string prompt = $"Please answer exclusively in {language}.\n\nContext:\n{contextBuilder}\n\nQuestion: {userQuery}\nDonne bien ta réponse exclusivement en {language}.\nAnswer:";

            var requestBody = new
            {
                model = selectedModel,
                prompt = prompt,
                stream = true,
                max_tokens = maxTokens
            };

            string json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl) { Content = content };
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                try
                {
                    using JsonDocument doc = JsonDocument.Parse(line);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("response", out JsonElement responsePart))
                    {
                        string text = responsePart.GetString();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            onUpdate?.Invoke(text);
                        }
                    }
                }
                catch
                {
                    // Ignore error here and skip
                }
            }
        }
    }
}
