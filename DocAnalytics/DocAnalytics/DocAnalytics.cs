using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.Json;
using System.Text;



namespace DocAnalytics
{
    public partial class DocAnalytics_Main : Form
    {
        public const int _iDefaultChunkSize = 200;
        public const int _iDefaultTimeoutSeconds = 1200;                    // Timeout SQL
        public const string _sEmbeddingsModel = "nomic-embed-text";         // Or "snowflake-arctic-embed2"

        private string _sConnectionString = "Server=ROCINANTE;Database=Digital_Library;UID=XXX;PWD=XXX;TrustServerCertificate=True;";
        private SqlConnection oSQLConnection;
        private ComboBox comboBox_TextType;
        private FlowLayoutPanel flowLayoutPanel_Images;        

        // NOTES :
        // ollama pull nomic-embed-text
        // ollama serve => To launch the console


        public DocAnalytics_Main()
        {
            InitializeComponent();
            InitializeDatabaseControls();
        }

        private void InitializeDatabaseControls()
        {
            button_Connect = new Button() { Text = "Connect", Location = new System.Drawing.Point(20, 20) };
            button_Disconnect = new Button() { Text = "Disconnect", Location = new System.Drawing.Point(120, 20) };
            
            label_Status = new Label() { Text = "Status: Disconnected", Location = new System.Drawing.Point(220, 25), Font = new System.Drawing.Font("Arial", 12, FontStyle.Bold | FontStyle.Underline), ForeColor = Color.DarkBlue, Width = 200 };

            button_Disconnect.Enabled = false;

            button_Connect.Click += Button_Connect_Click;
            button_Disconnect.Click += Button_Disconnect_Click;

            tabPage_Connection.Controls.Add(button_Connect);
            tabPage_Connection.Controls.Add(button_Disconnect);
            tabPage_Connection.Controls.Add(label_Status);

            textBox_PageContent = new TextBox() { Location = new System.Drawing.Point(20, 140), Width = 900, Height = 500, Multiline = true, ScrollBars = ScrollBars.Vertical };

            flowLayoutPanel_Images = new FlowLayoutPanel()
            {
                Location = new System.Drawing.Point(20, 680),
                Width = 800,
                Height = 200,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            Label label_Books = new Label() { Text = "Books :", Location = new System.Drawing.Point(20, 20), AutoSize = true };
            Label label_Pages = new Label() { Text = "Page :", Location = new System.Drawing.Point(20, 60), AutoSize = true };
            Label label_TextType = new Label() { Text = "Format :", Location = new System.Drawing.Point(20, 100), AutoSize = true };

            comboBox_Books = new ComboBox() { Location = new System.Drawing.Point(80, 20), Width = 350 };
            comboBox_Pages = new ComboBox() { Location = new System.Drawing.Point(80, 60), Width = 50 };

            Button button_DeleteBook = new Button() { Text = "Delete", Location = new System.Drawing.Point(450, 20), Width = 80 };
            button_DeleteBook.Click += Button_DeleteBook_Click;

            comboBox_Books.SelectedIndexChanged += ComboBox_Books_SelectedIndexChanged;
            comboBox_Pages.SelectedIndexChanged += ComboBox_Pages_SelectedIndexChanged;

            comboBox_TextType = new ComboBox() { Location = new System.Drawing.Point(80, 100), Width = 200 };
            comboBox_TextType.Items.AddRange(new string[] { "page_raw_text", "page_ocr_text", "page_raw_text_cleaned", "page_raw_text_llm", "page_raw_text_llm_explain" });
            comboBox_TextType.SelectedIndex = 0;  // Sélection par défaut
            comboBox_TextType.SelectedIndexChanged += ComboBox_Pages_SelectedIndexChanged; // Rafraîchir le texte affiché

            tabPage_Explore.Controls.Add(label_Books);
            tabPage_Explore.Controls.Add(comboBox_Books);
            tabPage_Explore.Controls.Add(button_DeleteBook);
            tabPage_Explore.Controls.Add(label_Pages);
            tabPage_Explore.Controls.Add(comboBox_Pages);
            tabPage_Explore.Controls.Add(label_TextType);
            tabPage_Explore.Controls.Add(comboBox_TextType);
            tabPage_Explore.Controls.Add(textBox_PageContent);

            tabPage_Explore.Controls.Add(flowLayoutPanel_Images);


            // =======================================================
            // tabPage_Embeddings
            // =======================================================

            Label label_Books_Embeddings = new Label() { Text = "Books :", Location = new System.Drawing.Point(20, 20), AutoSize = true };

            ComboBox cmbBooksWithStats = new ComboBox();
            cmbBooksWithStats.Name = "cmbBooksWithStats";
            cmbBooksWithStats.Location = new Point(80, 18);
            cmbBooksWithStats.Width = 600;            

            // Bouton "Compute Embeddings"
            Button btnComputeEmbeddings = new Button();
            btnComputeEmbeddings.Text = "Compute Embeddings";
            btnComputeEmbeddings.Location = new Point(700, 18);
            btnComputeEmbeddings.Click += btnComputeEmbeddings_Click;

            ProgressBar progressBar_Embeddings = new ProgressBar()
            {
                Name = "progressBar_Embeddings",
                Location = new Point(80, 60),
                Width = 600,
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };

            Button btnDeleteEmbeddings = new Button();
            btnDeleteEmbeddings.Text = "Delete Embeddings";
            btnDeleteEmbeddings.Location = new Point(800, 18);
            btnDeleteEmbeddings.Click += btnDeleteEmbeddings_Click;

            tabPage_Embeddings.Controls.Add(label_Books_Embeddings);
            tabPage_Embeddings.Controls.Add(cmbBooksWithStats);
            tabPage_Embeddings.Controls.Add(btnComputeEmbeddings);
            tabPage_Embeddings.Controls.Add(progressBar_Embeddings);
            tabPage_Embeddings.Controls.Add(btnDeleteEmbeddings);

            // =======================================================
            // tabPage_Ask
            // =======================================================

            Label label_AskRequest = new Label()
            {
                Text = "Enter here your request : ",
                Location = new System.Drawing.Point(20, 30),
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 12, FontStyle.Bold | FontStyle.Underline),
                ForeColor = Color.DarkBlue
            };

            TextBox txtSearchQuery = new TextBox()
            {
                Name = "txtSearchQuery",
                Location = new Point(20, 70),
                Width = 630
            };
            Button btnSearchEmbeddings = new Button()
            {
                Text = "Ask anything...",
                Location = new Point(670, 70),
                Width = 150
            };
            btnSearchEmbeddings.Click += btnSearchEmbeddings_Click;

            txtSearchQuery.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    btnSearchEmbeddings.PerformClick(); 
                    e.SuppressKeyPress = true;
                }
            };

            Label lblModel = new Label() { Text = "Selected Model :", Location = new Point(20, 110), AutoSize = true };
            ComboBox cmbModel = new ComboBox() { Name = "cmbModel", Location = new Point(125, 108), Width = 150 };
            cmbModel.Items.AddRange(new string[] { "mistral", "gemma:7b", "gemma3:27b", "deepseek:8b", "mixtral:8x7b", "deepseek:70b"});
            cmbModel.SelectedIndex = 0;

            Label lblTopN = new Label() { Text = "Top N :", Location = new Point(285, 110), AutoSize = true };
            ComboBox cmbTopN = new ComboBox() { Name = "cmbTopN", Location = new Point(340, 108), Width = 60 };
            cmbTopN.Items.AddRange(Enumerable.Range(1, 10).Select(i => i.ToString()).ToArray());
            cmbTopN.SelectedIndex = 2; // Top 3 par défaut

            Label lblLang = new Label() { Text = "Language :", Location = new Point(450, 110), AutoSize = true };
            ComboBox cmbLang = new ComboBox() { Name = "cmbLang", Location = new Point(530, 108), Width = 120 };
            cmbLang.Items.AddRange(new string[] { "Français", "English", "Dutch" });
            cmbLang.SelectedIndex = 0;

            Label lblTokens = new Label() { Text = "Max Tokens :", Location = new Point(20, 155), AutoSize = true };
            ComboBox cmbMaxTokens = new ComboBox() { Name = "cmbMaxTokens", Location = new Point(125, 153), Width = 80 };
            cmbMaxTokens.Items.AddRange(new string[] { "2048", "4096", "8192", "12000", "20000", "32000", "131075" });
            cmbMaxTokens.SelectedIndex = 1; // Default : 4096

            Label lblBook = new Label() { Text = "Target :", Location = new Point(285, 155), AutoSize = true };
            ComboBox cmbTargetBook = new ComboBox() { Name = "cmbTargetBook", Location = new Point(340, 153), Width = 310 };
            cmbTargetBook.Items.Add("All Books");
            cmbTargetBook.SelectedIndex = 0;

            Label label_Chunks = new Label()
            {
                Text = "Chunks Found : ",
                Location = new System.Drawing.Point(20, 240),
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 12, FontStyle.Bold | FontStyle.Underline),
                ForeColor = Color.DarkBlue
            };

            ListBox lstSearchResults = new ListBox()
            {
                Name = "lstSearchResults",
                Location = new Point(20, 280),
                Width = 800, 
                Height = 150,
                HorizontalScrollbar = true 
            };
            FlowLayoutPanel flowLayoutPanel_AskImages = new FlowLayoutPanel()
            {
                Name = "flowLayoutPanel_AskImages",
                Location = new Point(850, 150),
                Width = 800,
                Height = 800,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            Label label_LLM_Results = new Label()
            {
                Text = "LLM Answer : ",
                Location = new System.Drawing.Point(20, 470),
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 12, FontStyle.Bold | FontStyle.Underline),
                ForeColor = Color.DarkBlue
            };

            TextBox txtLLMResponse = new TextBox()
            {
                Name = "txtLLMResponse",
                Location = new Point(20, 510),
                Width = 800,
                Height = 250,
                Font = new System.Drawing.Font("Arial", 16),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            lstSearchResults.DoubleClick += (sender, e) =>
            {
                ListBox listBox = (ListBox)sender;
                if (listBox.SelectedItem is ListBox_Results_Helper selectedItem)
                {
                    TextBox txtSearchQuery = (TextBox)tabPage_Ask.Controls["txtSearchQuery"];
                    string searchQuery = txtSearchQuery.Text;

                    Form_RichTextDisplay displayForm = new Form_RichTextDisplay($"{selectedItem.BookName} - Page {selectedItem.Page_Number} - Similarity {selectedItem.Score}", selectedItem.FullText, searchQuery);
                    displayForm.ShowDialog();
                }
            };
            lstSearchResults.SelectedIndexChanged += (s, e) =>
            {
                if (lstSearchResults.SelectedItem is ListBox_Results_Helper selectedItem)
                {
                    DisplayImagesForPage(selectedItem.PageId);
                }
            };
            Label lblModelInfo = new Label()
            {
                Name = "lblModelInfo",
                Text = "WARNING : Token limits :\n\nMistral = 16k\nMixtral = 32k\nDeepSeek = 16k\nGemma3 = 128k\ndeepseek-r1:70b = 128k",
                Location = new Point(20, 780),
                AutoSize = true,
                Width = 880,
                Height = 40,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };

            tabPage_Ask.Controls.Add(label_AskRequest);
            tabPage_Ask.Controls.Add(txtSearchQuery);
            tabPage_Ask.Controls.Add(btnSearchEmbeddings);
            tabPage_Ask.Controls.Add(lblModel);
            tabPage_Ask.Controls.Add(cmbModel);
            tabPage_Ask.Controls.Add(lblTopN);
            tabPage_Ask.Controls.Add(cmbTopN);
            tabPage_Ask.Controls.Add(lblLang);
            tabPage_Ask.Controls.Add(cmbLang);
            tabPage_Ask.Controls.Add(lblTokens);
            tabPage_Ask.Controls.Add(cmbMaxTokens);
            tabPage_Ask.Controls.Add(lblBook);
            tabPage_Ask.Controls.Add(cmbTargetBook);
            tabPage_Ask.Controls.Add(label_Chunks);
            tabPage_Ask.Controls.Add(lstSearchResults);
            tabPage_Ask.Controls.Add(flowLayoutPanel_AskImages);
            tabPage_Ask.Controls.Add(label_LLM_Results);
            tabPage_Ask.Controls.Add(txtLLMResponse);
            tabPage_Ask.Controls.Add(lblModelInfo);
        }


        // =======================================================
        // tabPage_Connection
        // =======================================================
        private void Button_Connect_Click(object sender, EventArgs e)
        {
            try
            {
                label_Status.Text = "Status : Connecting...";

                if (oSQLConnection == null || oSQLConnection.State == ConnectionState.Closed)
                {
                    oSQLConnection = new SqlConnection(_sConnectionString);
                    oSQLConnection.Open();
                    label_Status.Text = "Status: Connected";
                    button_Connect.Enabled = false;
                    button_Disconnect.Enabled = true;
                }

                LoadBooks();
                LoadBooksWithStats();
                LoadBooksWithEmbeddings();

                ComboBox cmbBooksWithStats = (ComboBox)tabPage_Embeddings.Controls["cmbBooksWithStats"];
                if (cmbBooksWithStats.Items.Count > 0)
                {
                    cmbBooksWithStats.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message);
            }
        }

        private void Button_Disconnect_Click(object sender, EventArgs e)
        {
            if (oSQLConnection != null && oSQLConnection.State == ConnectionState.Open)
            {
                oSQLConnection.Close();
                label_Status.Text = "Status: Disconnected";
                button_Connect.Enabled = true;
                button_Disconnect.Enabled = false;
            }
        }
        private bool IsDatabaseConnected()
        {
            if (oSQLConnection == null || oSQLConnection.State == ConnectionState.Closed)
            {
                MessageBox.Show("Please connect to the database first.");
                return false;
            }
            return true;
        }

        private void LoadBooks()
        {
            if (!IsDatabaseConnected()) return;

            comboBox_Books.Items.Clear();
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT b.book_id_pkey, b.book_title, COUNT(p.page_id_pkey) AS page_count
                FROM Books b
                LEFT JOIN Pages p ON b.book_id_pkey = p.page_book_id_fkey
                GROUP BY b.book_id_pkey, b.book_title ORDER BY b.book_title", oSQLConnection))
            {
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int bookId = reader.GetInt32(0);
                    string bookTitle = reader.GetString(1);
                    int pageCount = reader.GetInt32(2); // Real page count

                    comboBox_Books.Items.Add(new { Id = bookId, Title = $"{bookTitle} ({pageCount} page(s))", PageCount = pageCount });
                }
                reader.Close();
            }

            comboBox_Books.DisplayMember = "Title";
            comboBox_Books.ValueMember = "Id";

            if (comboBox_Books.Items.Count > 0)
            {
                comboBox_Books.SelectedIndex = 0;
            }
        }

        private void ComboBox_Books_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_Books.SelectedItem != null)
            {
                int bookId = ((dynamic)comboBox_Books.SelectedItem).Id;
                LoadPages(bookId);

                if (comboBox_Pages.Items.Count > 0)
                {
                    comboBox_Pages.SelectedIndex = 0;
                }
            }
        }

        private void ClearPageData()
        {
            comboBox_Pages.Items.Clear(); 
            textBox_PageContent.Text = "";
            flowLayoutPanel_Images.Controls.Clear();
        }

        private void LoadPages(int bookId)
        {
            if (!IsDatabaseConnected()) return;

            comboBox_Pages.Items.Clear();
            using (SqlCommand cmd = new SqlCommand("SELECT page_id_pkey, page_number FROM Pages WHERE page_book_id_fkey = @bookId ORDER BY page_number", oSQLConnection))
            {
                cmd.Parameters.AddWithValue("@bookId", bookId);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    comboBox_Pages.Items.Add(new { Id = reader.GetInt32(0), Number = reader.GetInt32(1) });
                }
                reader.Close();
            }

            comboBox_Pages.DisplayMember = "Number";
            comboBox_Pages.ValueMember = "Id";

            if (comboBox_Pages.Items.Count > 0)
            {
                comboBox_Pages.SelectedIndex = 0;
            }
            else
            {
                ClearPageData();
            }
        }


        private void Button_DeleteBook_Click(object sender, EventArgs e)
        {
            if (!IsDatabaseConnected()) return;

            if (comboBox_Books.SelectedItem == null) return;

            int bookId = ((dynamic)comboBox_Books.SelectedItem).Id;
            string bookTitle = comboBox_Books.Text;

            DialogResult result = MessageBox.Show(
                $"Are you sure you want to delete '{bookTitle}' and all its related pages and images?",
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM Images WHERE image_page_id_fkey IN (SELECT page_id_pkey FROM Pages WHERE page_book_id_fkey = @bookId); DELETE FROM Pages WHERE page_book_id_fkey = @bookId; DELETE FROM Books WHERE book_id_pkey = @bookId;", oSQLConnection))
                    {
                        cmd.Parameters.AddWithValue("@bookId", bookId);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show($"The book '{bookTitle}' has been deleted.", "Deletion Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadBooks();
                    LoadBooksWithEmbeddings();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting book: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ComboBox_Pages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_Pages.SelectedItem != null)
            {
                int pageId = ((dynamic)comboBox_Pages.SelectedItem).Id;
                LoadPageContent(pageId);
            }
        }
        private void LoadPageContent(int pageId)
        {
            if (!IsDatabaseConnected()) return;

            if (comboBox_TextType.SelectedItem == null) return;

            string selectedColumn = comboBox_TextType.SelectedItem.ToString(); 

            using (SqlCommand cmd = new SqlCommand($"SELECT {selectedColumn} FROM Pages WHERE page_id_pkey = @pageId", oSQLConnection))
            {
                cmd.Parameters.AddWithValue("@pageId", pageId);
                object result = cmd.ExecuteScalar();
                textBox_PageContent.Text = result != null ? result.ToString() : "No content available.";
            }

            LoadImagesForPage(pageId);
        }

        private void LoadImagesForPage(int pageId)
        {
            flowLayoutPanel_Images.Controls.Clear();

            if (!IsDatabaseConnected()) return;

            using (SqlCommand cmd = new SqlCommand("SELECT image_data FROM Images WHERE image_page_id_fkey = @pageId", oSQLConnection))
            {
                cmd.Parameters.AddWithValue("@pageId", pageId);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    byte[] imageData = (byte[])reader["image_data"];
                    PictureBox pictureBox = new PictureBox
                    {
                        Image = Image.FromStream(new MemoryStream(imageData)),
                        Width = 150,
                        Height = 150,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Margin = new Padding(5)
                    };
                    flowLayoutPanel_Images.Controls.Add(pictureBox);
                }
                reader.Close();
            }
        }


        // tabPage_Embeddings
        // ===================
        public void LoadBooksWithStats()
        {
            if (!IsDatabaseConnected()) return;

            ComboBox cmbBooksWithStats = (ComboBox)tabPage_Embeddings.Controls["cmbBooksWithStats"];
            cmbBooksWithStats.Items.Clear();

            using (SqlCommand cmd = new SqlCommand(@"
                    SELECT b.book_id_pkey, b.book_title, 
                           COUNT(DISTINCT p.page_id_pkey) AS total_pages, 
                           COUNT(e.embedding_id_pkey) AS total_embeddings,
                           COALESCE(SUM(ev_count.total_vectors), 0) AS total_vectors
                    FROM Books b
                    LEFT JOIN Pages p ON p.page_book_id_fkey = b.book_id_pkey
                    LEFT JOIN Embeddings e ON e.embedding_page_id_fkey = p.page_id_pkey
                    LEFT JOIN (
                        SELECT embval_embedding_id_fkey, COUNT(*) AS total_vectors
                        FROM Embeddings_Values
                        GROUP BY embval_embedding_id_fkey
                    ) AS ev_count ON e.embedding_id_pkey = ev_count.embval_embedding_id_fkey
                    GROUP BY b.book_id_pkey, b.book_title
                    ORDER BY b.book_title", oSQLConnection))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int bookId = reader.GetInt32(0);
                        string bookName = reader.GetString(1);
                        int totalPages = reader.GetInt32(2);
                        int totalEmbeddings = reader.GetInt32(3);
                        int totalVectors = reader.GetInt32(4);

                        BookHelper bookItem = new BookHelper
                        {
                            iBookId = bookId,
                            sDisplayName = $"{bookName} ({totalPages} pages, {totalEmbeddings} embeddings, {totalVectors} vectors)"
                        };

                        cmbBooksWithStats.Items.Add(bookItem);
                    }
                }
                if (cmbBooksWithStats.Items.Count > 0)
                {
                    cmbBooksWithStats.SelectedIndex = 0;
                }
                else
                {                    
                    cmbBooksWithStats.Items.Clear();
                }
            }
        }

        private async void btnComputeEmbeddings_Click(object sender, EventArgs e)
        {
            if (!IsDatabaseConnected()) return;

            ComboBox cmbBooksWithStats = (ComboBox)tabPage_Embeddings.Controls["cmbBooksWithStats"];
            if (cmbBooksWithStats.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner un livre.");
                return;
            }

            BookHelper selectedBook = (BookHelper)cmbBooksWithStats.SelectedItem;
            int bookId = selectedBook.iBookId;

            ProgressBar progressBar = (ProgressBar)tabPage_Embeddings.Controls["progressBar_Embeddings"];
            if (progressBar == null)
            {
                MessageBox.Show("La ProgressBar n'a pas été trouvée !");
                return;
            }

            progressBar.Value = 0; 

            Progress<int> progress = new Progress<int>(value =>
            {
                progressBar.Value = value; // Update ProgressBar
            });

            await OllamaHelper.ComputeEmbeddingsForBook(bookId, oSQLConnection, progress);

            LoadBooksWithStats();
            LoadBooksWithEmbeddings();
        }

        private void btnDeleteEmbeddings_Click(object sender, EventArgs e)
        {
            if (!IsDatabaseConnected()) return;

            ComboBox cmbBooksWithStats = (ComboBox)tabPage_Embeddings.Controls["cmbBooksWithStats"];
            if (cmbBooksWithStats.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner un livre.");
                return;
            }

            BookHelper selectedBook = (BookHelper)cmbBooksWithStats.SelectedItem;
            int bookId = selectedBook.iBookId;

            var confirmResult = MessageBox.Show(
                "Are you sure you want to delete all embeddings for this book?",
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirmResult == DialogResult.Yes)
            {
                OllamaHelper.DeleteExistingEmbeddings(bookId, oSQLConnection);
                MessageBox.Show("Embeddings deleted successfully.");
                LoadBooksWithStats(); // Update after suppression
                LoadBooksWithEmbeddings();
            }
        }


        // =======================================================
        // tabPage_Ask
        // =======================================================

        public void LoadBooksWithEmbeddings()
        {
            if (!IsDatabaseConnected()) return;

            ComboBox cmbTargetBook = (ComboBox)tabPage_Ask.Controls["cmbTargetBook"];
            cmbTargetBook.Items.Clear();
            cmbTargetBook.Items.Add("All Books");

            string query = @"
                SELECT DISTINCT b.book_title
                FROM Books b
                INNER JOIN Pages p ON p.page_book_id_fkey = b.book_id_pkey
                INNER JOIN Embeddings e ON e.embedding_page_id_fkey = p.page_id_pkey";

            using (SqlCommand cmd = new SqlCommand(query, oSQLConnection))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    cmbTargetBook.Items.Add(reader.GetString(0));
                }
            }

            cmbTargetBook.SelectedIndex = 0;
        }

        private string HighlightQueryText(string fullText, string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return fullText;

            string[] queryWords = query.Split(' ');
            foreach (string word in queryWords)
            {
                fullText = fullText.Replace(word, $"**{word}**", StringComparison.OrdinalIgnoreCase);
            }

            return fullText;
        }

        public static async Task<float[]> GenerateQueryEmbedding(string query)
        {
            return await OllamaHelper.ComputeEmbeddingOllama(query);
        }

        private async void btnSearchEmbeddings_Click(object sender, EventArgs e)
        {
            FlowLayoutPanel panel = (FlowLayoutPanel)tabPage_Ask.Controls["flowLayoutPanel_AskImages"];
            panel.Controls.Clear();

            if (!IsDatabaseConnected()) return;

            TextBox txtSearchQuery = (TextBox)tabPage_Ask.Controls["txtSearchQuery"];
            ListBox lstSearchResults = (ListBox)tabPage_Ask.Controls["lstSearchResults"];
            TextBox txtLLMResponse = (TextBox)tabPage_Ask.Controls["txtLLMResponse"];

            if (string.IsNullOrWhiteSpace(txtSearchQuery.Text))
            {
                MessageBox.Show("Please enter a valid request.");
                return;
            }

            string sSelectedModel = ((ComboBox)tabPage_Ask.Controls["cmbModel"]).SelectedItem.ToString();
            int iTopN = int.Parse(((ComboBox)tabPage_Ask.Controls["cmbTopN"]).SelectedItem.ToString());
            string sLangage = ((ComboBox)tabPage_Ask.Controls["cmbLang"]).SelectedItem.ToString();
            int iMaxTokens = int.Parse(((ComboBox)tabPage_Ask.Controls["cmbMaxTokens"]).SelectedItem.ToString());
            string sTargetBook = ((ComboBox)tabPage_Ask.Controls["cmbTargetBook"]).SelectedItem.ToString(); // "All Books" ou un titre

            lstSearchResults.Items.Clear();
            txtLLMResponse.Text = "Processing in progress...";

            Form_Progress progressForm = new Form_Progress();
            progressForm.SetMessage("Calculating the question embedding...");
            progressForm.SetProgressBar(0);
            progressForm.Show();

            try
            {
                // Step 1: Generate the user request embedding
                float[] queryEmbedding = await OllamaHelper.GenerateQueryEmbedding(txtSearchQuery.Text);
                progressForm.SetMessage("Searching for vector occurrences in database...");
                progressForm.SetProgressBar(10);

                // Step 2: Run the search for the most relevant pages
                List<(int, string, string, string, string, string, string, float, int)> results =
                    OllamaHelper.SearchEmbeddings(oSQLConnection, queryEmbedding, iTopN, sTargetBook);
                progressForm.SetMessage("Processing the results...");
                progressForm.SetProgressBar(75);

                // Step 3: Display the results in the ListBox
                lstSearchResults.Items.Clear();
                List<(string, string)> context = new List<(string, string)>();

                foreach (var result in results)
                {
                    int pageId = result.Item1;
                    string bookName = result.Item2;
                    string textType = result.Item3;
                    string fullText = result.Item4;
                    string cleanedText = result.Item5;
                    string llmText = result.Item6; 
                    float score = result.Item8;
                    int page_number = result.Item9;

                    context.Add((cleanedText, llmText));

                    string displayText = $"{llmText.Substring(0, Math.Min(llmText.Length, 10000))}...";

                    lstSearchResults.Items.Add(new ListBox_Results_Helper(displayText, bookName, pageId, textType, fullText, score, page_number));
                }

                if (lstSearchResults.Items.Count == 0)
                {
                    lstSearchResults.Items.Add("Sorry, no result...");
                }

                txtLLMResponse.Clear();
                progressForm.SetMessage("Generating answer of the LLM...");
                progressForm.SetProgressBar(75);

                await OllamaHelper.QueryOllamaMistralStreamed(
                    txtSearchQuery.Text,
                    context,
                    sSelectedModel,
                    iMaxTokens,
                    sLangage,
                    (string chunk) =>
                    {
                        txtLLMResponse.Invoke((MethodInvoker)(() =>
                        {
                            if (progressForm.Visible)
                                progressForm.Close();
                            txtLLMResponse.AppendText(chunk);
                        }));
                    });
            }
            catch (Exception ex)
            {
                progressForm.Close();
                MessageBox.Show("Search error : " + ex.Message);
            }
        }


        private void DisplayImagesForPage(int pageId)
        {
            if (!IsDatabaseConnected()) return;

            FlowLayoutPanel panel = (FlowLayoutPanel)tabPage_Ask.Controls["flowLayoutPanel_AskImages"];
            panel.Controls.Clear();

            string query = "SELECT image_data FROM Images WHERE image_page_id_fkey = @PageId";

            using (SqlConnection conn = new SqlConnection(_sConnectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@PageId", pageId);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        byte[] imageBytes = (byte[])reader[0];
                        using (MemoryStream ms = new MemoryStream(imageBytes))
                        {
                            PictureBox pb = new PictureBox
                            {
                                Image = Image.FromStream(ms),
                                SizeMode = PictureBoxSizeMode.Zoom,
                                Width = 300,      
                                Height = 300,     
                                Margin = new Padding(10),
                                Cursor = Cursors.Hand,
                                Tag = imageBytes       // => For enlargement
                            };

                            pb.DoubleClick += (s, e) =>
                            {
                                PictureBox clicked = (PictureBox)s;
                                byte[] data = (byte[])clicked.Tag;

                                using (MemoryStream msZoom = new MemoryStream(data))
                                {
                                    Form viewer = new Form
                                    {
                                        Text = "Image Preview",
                                        Width = 800,
                                        Height = 800,
                                        StartPosition = FormStartPosition.CenterParent
                                    };

                                    PictureBox fullImage = new PictureBox
                                    {
                                        Image = Image.FromStream(msZoom),
                                        Dock = DockStyle.Fill,
                                        SizeMode = PictureBoxSizeMode.Zoom
                                    };

                                    viewer.Controls.Add(fullImage);
                                    viewer.ShowDialog();
                                }
                            };

                            panel.Controls.Add(pb);
                        }
                    }
                }
            }
        }

    }
}

