using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace RecipeManager
{
    public partial class FormMain : Form
    {
        private string _filterCuisine = "";
        private string _filterMeal = "";
        private string _filterDifficulty = "";
        private string _filterCookTime = "";
        private string _filterIsMade = "";
        private bool _filterPanelOpen = false;

        private CheckBox[] _chkCuisine, _chkMeal, _chkDifficulty, _chkCookTime, _chkIsMade;
        private int _selectedRecipeID = -1;

        private Panel pnlTop, pnlFilter, pnlBottom;
        private TextBox txtSearch;
        private Button btnFilter, btnAdd, btnEdit, btnDelete;
        private DataGridView dgv;
        private Label lblCount;

        public FormMain()
        {
            InitializeComponent();
            BuildUI();
            LoadRecipes();
        }

        private void BuildUI()
        {
            this.Text = "食譜管理 Recipe Manager";
            this.Size = new Size(900, 600);
            this.MinimumSize = new Size(700, 500);
            this.BackColor = Color.FromArgb(245, 244, 241);
            this.Font = new Font("微軟正黑體", 10f);
            this.StartPosition = FormStartPosition.CenterScreen;

            // ===== 頂部列 =====
            pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = Color.White,
            };
            pnlTop.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Color.FromArgb(30, 0, 0, 0)),
                    0, pnlTop.Height - 1, pnlTop.Width, pnlTop.Height - 1);
            this.Controls.Add(pnlTop);

            var lblTitle = new Label
            {
                Text = "食譜管理   Recipe Manager",
                Font = new Font("微軟正黑體", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize = true,
                Location = new Point(24, 12)
            };
            pnlTop.Controls.Add(lblTitle);

            btnFilter = CreateStyledButton("▼  篩選", 24, 46, 90, 34);
            btnFilter.Click += BtnFilter_Click;
            pnlTop.Controls.Add(btnFilter);

            txtSearch = new TextBox
            {
                Location = new Point(124, 46),
                Size = new Size(pnlTop.Width - 160, 34),
                Font = new Font("微軟正黑體", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(50, 50, 50)
            };
            txtSearch.TextChanged += (s, e) => LoadRecipes();
            pnlTop.Controls.Add(txtSearch);
            pnlTop.Resize += (s, e) => txtSearch.Width = pnlTop.Width - 160;

            // ===== 篩選面板 =====
            pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 0,
                BackColor = Color.White,
                Visible = false
            };
            pnlFilter.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Color.FromArgb(30, 0, 0, 0)),
                    0, pnlFilter.Height - 1, pnlFilter.Width, pnlFilter.Height - 1);
            this.Controls.Add(pnlFilter);
            BuildFilterPanel();

            // ===== DataGridView =====
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(245, 244, 241),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
                GridColor = Color.FromArgb(230, 230, 230),
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                Font = new Font("微軟正黑體", 10f),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
            };
            dgv.RowTemplate.Height = 48;
            dgv.ColumnHeadersHeight = 36;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(130, 130, 130);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("微軟正黑體", 9f);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(30, 30, 30);
            dgv.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(238, 237, 254);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(60, 52, 137);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(251, 251, 251);

            dgv.SelectionChanged += Dgv_SelectionChanged;
            dgv.CellDoubleClick += Dgv_CellDoubleClick;
            this.Controls.Add(dgv);
            BuildDgvColumns();

            // ===== 底部列 =====
            pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.White
            };
            pnlBottom.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Color.FromArgb(30, 0, 0, 0)),
                    0, 0, pnlBottom.Width, 0);
            this.Controls.Add(pnlBottom);

            lblCount = new Label
            {
                Text = "共 0 道食譜",
                AutoSize = true,
                ForeColor = Color.FromArgb(130, 130, 130),
                Font = new Font("微軟正黑體", 9f),
                Location = new Point(24, 22)
            };
            pnlBottom.Controls.Add(lblCount);

            var lblTip = new Label
            {
                Text = "雙擊食譜可查看詳細內容",
                AutoSize = true,
                ForeColor = Color.FromArgb(170, 170, 170),
                Font = new Font("微軟正黑體", 9f),
                Location = new Point(160, 22)
            };
            pnlBottom.Controls.Add(lblTip);

            btnDelete = CreateStyledButton("🗑  刪除", 0, 0, 96, 34);
            btnDelete.ForeColor = Color.FromArgb(163, 45, 45);
            btnDelete.FlatAppearance.BorderColor = Color.FromArgb(240, 149, 149);
            btnDelete.MouseEnter += (s, e) => btnDelete.BackColor = Color.FromArgb(252, 235, 235);
            btnDelete.MouseLeave += (s, e) => btnDelete.BackColor = Color.White;
            btnDelete.Click += BtnDelete_Click;
            btnDelete.Enabled = false;
            pnlBottom.Controls.Add(btnDelete);

            btnEdit = CreateStyledButton("✏  編輯", 0, 0, 96, 34);
            btnEdit.Click += BtnEdit_Click;
            btnEdit.Enabled = false;
            pnlBottom.Controls.Add(btnEdit);

            btnAdd = CreateStyledButton("＋  新增", 0, 0, 96, 34);
            btnAdd.BackColor = Color.FromArgb(83, 74, 183);
            btnAdd.ForeColor = Color.FromArgb(238, 237, 254);
            btnAdd.FlatAppearance.BorderColor = Color.FromArgb(83, 74, 183);
            btnAdd.MouseEnter += (s, e) => btnAdd.BackColor = Color.FromArgb(60, 52, 137);
            btnAdd.MouseLeave += (s, e) => btnAdd.BackColor = Color.FromArgb(83, 74, 183);
            btnAdd.Click += BtnAdd_Click;
            pnlBottom.Controls.Add(btnAdd);

            pnlBottom.Resize += (s, e) => LayoutBottomBtns();
            this.Shown += (s, e) => LayoutBottomBtns();

            pnlBottom.BringToFront();
            pnlFilter.BringToFront();
            pnlTop.BringToFront();
        }

        private void LayoutBottomBtns()
        {
            int y = (pnlBottom.Height - 34) / 2;
            btnAdd.Location    = new Point(pnlBottom.Width - 24 - 96, y);
            btnEdit.Location   = new Point(pnlBottom.Width - 24 - 96 - 106, y);
            btnDelete.Location = new Point(pnlBottom.Width - 24 - 96 - 212, y);
        }

        private void BuildDgvColumns()
        {
            dgv.AutoGenerateColumns = false;
            void AddCol(string name, string header, int weight, bool visible = true)
            {
                var col = new DataGridViewTextBoxColumn
                {
                    Name = name,
                    HeaderText = header,
                    FillWeight = weight,
                    Visible = visible,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                };
                dgv.Columns.Add(col);
            }
            AddCol("RecipeID", "", 1, false);
            AddCol("Name", "料理名稱", 28);
            AddCol("CuisineType", "類型", 13);
            AddCol("MealType", "餐別", 13);
            AddCol("Difficulty", "難度", 12);
            AddCol("CookTime", "時間", 12);
            AddCol("Rating", "評分", 12);
            AddCol("IsMade", "做過", 10);
        }

        private void BuildFilterPanel()
        {
            int x = 20, y = 14, colW = 155;

            AddFilterGroup("料理類型", new[] { "中式料理", "西式料理", "其他" }, ref _chkCuisine, x, y, colW);
            x += colW + 16;
            AddFilterGroup("餐別", new[] { "早餐", "午餐", "晚餐", "點心", "飲料" }, ref _chkMeal, x, y, colW);
            x += colW + 16;
            AddFilterGroup("料理時間", new[] { "15 分鐘內", "15–30 分鐘", "30–60 分鐘", "60 分鐘以上" }, ref _chkCookTime, x, y, colW);
            x += colW + 16;
            AddFilterGroup("難度", new[] { "簡單", "普通", "困難" }, ref _chkDifficulty, x, y, colW);
            x += colW + 16;
            AddFilterGroup("是否做過", new[] { "做過", "未做過" }, ref _chkIsMade, x, y, colW);
        }

        private void AddFilterGroup(string title, string[] options,
            ref CheckBox[] arr, int x, int y, int colW)
        {
            pnlFilter.Controls.Add(new Label
            {
                Text = title,
                AutoSize = true,
                Font = new Font("微軟正黑體", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 120, 120),
                Location = new Point(x, y)
            });

            arr = new CheckBox[options.Length];
            for (int i = 0; i < options.Length; i++)
            {
                var chk = new CheckBox
                {
                    Text = options[i],
                    AutoSize = true,
                    Font = new Font("微軟正黑體", 10f),
                    ForeColor = Color.FromArgb(40, 40, 40),
                    Location = new Point(x, y + 22 + i * 27),
                    Tag = options[i]
                };
                chk.CheckedChanged += (s, e) => ApplyFilter();
                pnlFilter.Controls.Add(chk);
                arr[i] = chk;
            }
        }

        private void ApplyFilter()
        {
            _filterCuisine    = GetChecked(_chkCuisine);
            _filterMeal       = GetChecked(_chkMeal);
            _filterDifficulty = GetChecked(_chkDifficulty);
            _filterIsMade     = GetChecked(_chkIsMade);

            var ct = GetChecked(_chkCookTime);
            if (ct == "15 分鐘內")      _filterCookTime = "15以內";
            else if (ct == "15–30 分鐘") _filterCookTime = "15-30";
            else if (ct == "30–60 分鐘") _filterCookTime = "30-60";
            else if (ct == "60 分鐘以上") _filterCookTime = "60以上";
            else                          _filterCookTime = "";

            LoadRecipes();
        }

        private string GetChecked(CheckBox[] arr)
        {
            if (arr == null) return "";
            foreach (var c in arr)
                if (c.Checked) return c.Tag.ToString();
            return "";
        }

        private void LoadRecipes()
        {
            // 1. 解除事件繫結，避免干擾
            dgv.SelectionChanged -= Dgv_SelectionChanged;

            try
            {
                var list = DBHelper.GetRecipes(
                    txtSearch?.Text ?? "",
                    _filterCuisine, _filterMeal,
                    _filterDifficulty, _filterCookTime, _filterIsMade);

                // 2. 不要用手動 dgv.Rows.Add() 了！
                // 我們建立一個符合 DataGridView 顯示格式的 DataTable
                DataTable dt = new DataTable();
                dt.Columns.Add("RecipeID", typeof(int));
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("CuisineType", typeof(string));
                dt.Columns.Add("MealType", typeof(string));
                dt.Columns.Add("Difficulty", typeof(string));
                dt.Columns.Add("CookTime", typeof(string));
                dt.Columns.Add("Rating", typeof(string));
                dt.Columns.Add("IsMade", typeof(string));

                // 3. 先插入一列隱形佔位列（純程式碼，不存入資料庫）
                dt.Rows.Add(DBHelper.PlaceholderID, "", "", "", "", "", "", "");

                // 4. 把撈出來的 list 資料塞進 DataTable（真實食譜從第1列開始，全部可見）
                foreach (var r in list)
                {
                    dt.Rows.Add(
                        r.RecipeID,
                        r.Name,
                        r.CuisineType,
                        r.MealType,
                        r.Difficulty,
                        r.CookTime > 0 ? r.CookTime + " 分鐘" : "—",
                        r.Rating > 0 ? "★ " + r.Rating + " / 10" : "—",
                        r.IsMade ? "✓ 做過" : "— 未做過"
                    );
                }

                // 5. 重要步驟：將 DataGridView 的各個 Column 的 DataPropertyName 與 DataTable 的欄位名稱綁定
                dgv.Columns["RecipeID"].DataPropertyName = "RecipeID";
                dgv.Columns["Name"].DataPropertyName = "Name";
                dgv.Columns["CuisineType"].DataPropertyName = "CuisineType";
                dgv.Columns["MealType"].DataPropertyName = "MealType";
                dgv.Columns["Difficulty"].DataPropertyName = "Difficulty";
                dgv.Columns["CookTime"].DataPropertyName = "CookTime";
                dgv.Columns["Rating"].DataPropertyName = "Rating";
                dgv.Columns["IsMade"].DataPropertyName = "IsMade";

                // 5. 直接將 DataTable 給予 DataSource，讓系統自動完美繪製
                dgv.DataSource = dt;

                // 6. 清除預設選取
                dgv.ClearSelection();

                if (lblCount != null)
                    lblCount.Text = $"共 {list.Count} 道食譜";
            }
            catch (Exception ex)
            {
                MessageBox.Show("載入食譜失敗: " + ex.Message);
            }
            finally
            {
                // 7. 恢復事件繫結
                dgv.SelectionChanged += Dgv_SelectionChanged;
            }

            _selectedRecipeID = -1;
            UpdateButtonState();
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count > 0)
            {
                var val = dgv.SelectedRows[0].Cells["RecipeID"].Value;
                int id = (val != null) ? (int)val : -1;
                _selectedRecipeID = (id == DBHelper.PlaceholderID) ? -1 : id;
            }
            else
                _selectedRecipeID = -1;
            UpdateButtonState();
        }

        private void Dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var val = dgv.Rows[e.RowIndex].Cells["RecipeID"].Value;
            if (val == null) return;
            int id = (int)val;
            if (id == DBHelper.PlaceholderID) return;
            _selectedRecipeID = id;
            OpenDetail();
        }

        private void UpdateButtonState()
        {
            bool sel = _selectedRecipeID > 0 && _selectedRecipeID != DBHelper.PlaceholderID;
            btnEdit.Enabled   = sel;
            btnDelete.Enabled = sel;
        }

        private void BtnFilter_Click(object sender, EventArgs e)
        {
            _filterPanelOpen = !_filterPanelOpen;
            pnlFilter.Visible = _filterPanelOpen;
            pnlFilter.Height  = _filterPanelOpen ? 185 : 0;
            btnFilter.Text    = _filterPanelOpen ? "▲  篩選" : "▼  篩選";
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new FormEdit(null);
            if (form.ShowDialog() == DialogResult.OK)
                LoadRecipes();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (_selectedRecipeID < 0) return;
            var recipe = DBHelper.GetRecipeByID(_selectedRecipeID);
            var form = new FormEdit(recipe);
            if (form.ShowDialog() == DialogResult.OK)
                LoadRecipes();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedRecipeID < 0) return;
            var result = MessageBox.Show("確定要刪除這道食譜嗎？", "確認刪除",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                DBHelper.DeleteRecipe(_selectedRecipeID);
                LoadRecipes();
            }
        }

        private void OpenDetail()
        {
            if (_selectedRecipeID < 0) return;
            var recipe = DBHelper.GetRecipeByID(_selectedRecipeID);
            var form = new FormDetail(recipe);
            form.ShowDialog();
            LoadRecipes();
        }

        private Button CreateStyledButton(string text, int x, int y, int w, int h)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(40, 40, 40),
                Font = new Font("微軟正黑體", 9.5f),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(190, 190, 190);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(245, 244, 241);
            return btn;
        }
    }
}
