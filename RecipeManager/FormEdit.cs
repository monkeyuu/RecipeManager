using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RecipeManager
{
    public partial class FormEdit : Form
    {
        private RecipeItem _recipe;
        private bool _isEdit;
        private int _rating = 0;

        private TextBox txtName, txtCookTime, txtServings, txtSteps, txtNote;
        private ComboBox cboCuisine, cboMeal, cboDifficulty;
        private CheckBox chkIsMade;
        private Panel pnlIngredients;
        private Panel _scroll;
        private Panel _pnlBottom;
        private Button _btnSave, _btnCancel;
        private Button[] _stars = new Button[5];
        private Label lblRatingVal;

        public FormEdit(RecipeItem recipe)
        {
            InitializeComponent();
            _recipe = recipe;
            _isEdit = recipe != null;
            BuildUI();
            if (_isEdit) LoadData();
            this.Shown += (s, e) =>
            {
                if (_scroll != null) _scroll.AutoScrollPosition = new Point(0, 0);
                PositionBottomButtons();
            };
            this.Resize += (s, e) => PositionBottomButtons();
        }

        private void PositionBottomButtons()
        {
            if (_pnlBottom == null) return;
            int y2 = (_pnlBottom.Height - 34) / 2;
            if (_btnSave != null)   _btnSave.Location   = new Point(_pnlBottom.Width - 24 - 106, y2);
            if (_btnCancel != null) _btnCancel.Location = new Point(_pnlBottom.Width - 24 - 106 - 100, y2);
        }

        private void BuildUI()
        {
            this.Text = _isEdit ? "編輯食譜" : "新增食譜";
            this.Size = new Size(780, 680);
            this.MinimumSize = new Size(680, 580);
            this.BackColor = Color.FromArgb(245, 244, 241);
            this.Font = new Font("微軟正黑體", 10f);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // ===== 頂部 =====
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.White
            };
            pnlTop.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Color.FromArgb(30, 0, 0, 0)),
                    0, pnlTop.Height - 1, pnlTop.Width, pnlTop.Height - 1);
            this.Controls.Add(pnlTop);

            var btnBack = new Button
            {
                Text = "◀",
                Location = new Point(16, 12),
                Size = new Size(32, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(100, 100, 100),
                Font = new Font("微軟正黑體", 11f),
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btnBack.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            pnlTop.Controls.Add(btnBack);

            pnlTop.Controls.Add(new Label
            {
                Text = _isEdit ? "編輯食譜" : "新增食譜",
                Font = new Font("微軟正黑體", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize = true,
                Location = new Point(58, 16)
            });

            // ===== 底部 =====
            _pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                BackColor = Color.White
            };
            _pnlBottom.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Color.FromArgb(30, 0, 0, 0)),
                    0, 0, _pnlBottom.Width, 0);
            this.Controls.Add(_pnlBottom);
            var pnlBottom = _pnlBottom;

            _btnCancel = CreateBtn("取消", 0, 0, 90, 34);
            _btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            var btnCancel = _btnCancel;

            _btnSave = CreateBtn("儲存食譜", 0, 0, 106, 34);
            _btnSave.BackColor = Color.FromArgb(83, 74, 183);
            _btnSave.ForeColor = Color.FromArgb(238, 237, 254);
            _btnSave.FlatAppearance.BorderColor = Color.FromArgb(83, 74, 183);
            _btnSave.MouseEnter += (s, e) => _btnSave.BackColor = Color.FromArgb(60, 52, 137);
            _btnSave.MouseLeave += (s, e) => _btnSave.BackColor = Color.FromArgb(83, 74, 183);
            _btnSave.Click += BtnSave_Click;
            var btnSave = _btnSave;

            pnlBottom.Controls.Add(btnCancel);
            pnlBottom.Controls.Add(btnSave);

            // ===== 捲動主體 =====
            _scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 244, 241),
                Padding = new Padding(24, 20, 24, 20)
            };
            var scroll = _scroll;
            this.Controls.Add(scroll);

            int cardW = 700, cx = 0;

            // ---- 基本資訊卡 ----
            var cardBasic = MakeCard(cx, 0, cardW, 220, "基本資訊");
            cardBasic.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            scroll.Controls.Add(cardBasic);

            txtName = MakeTextBox(cardBasic, "料理名稱 *", 12, 44, cardW - 30, 34);

            var lblC = MakeLabel(cardBasic, "料理類型", 12, 98);
            cboCuisine = MakeCombo(cardBasic, new[] { "中式料理", "西式料理", "其他" }, 12, 118, 220);

            var lblM = MakeLabel(cardBasic, "餐別", 250, 98);
            cboMeal = MakeCombo(cardBasic, new[] { "早餐", "午餐", "晚餐", "點心", "飲料" }, 250, 118, 180);

            var lblD = MakeLabel(cardBasic, "難度", 448, 98);
            cboDifficulty = MakeCombo(cardBasic, new[] { "簡單", "普通", "困難" }, 448, 118, 150);

            MakeLabel(cardBasic, "烹飪時間（分鐘）", 12, 162);
            txtCookTime = new TextBox
            {
                Location = new Point(12, 182),
                Size = new Size(160, 30),
                Font = new Font("微軟正黑體", 10f),
                BorderStyle = BorderStyle.FixedSingle
            };
            cardBasic.Controls.Add(txtCookTime);

            MakeLabel(cardBasic, "幾人份", 190, 162);
            txtServings = new TextBox
            {
                Location = new Point(190, 182),
                Size = new Size(120, 30),
                Font = new Font("微軟正黑體", 10f),
                BorderStyle = BorderStyle.FixedSingle
            };
            cardBasic.Controls.Add(txtServings);

            // ---- 食材卡 ----
            var cardIng = MakeCard(cx, 236, cardW, 260, "食材清單");
            cardIng.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            scroll.Controls.Add(cardIng);

            var ingHeader = new Panel { Location = new Point(12, 42), Size = new Size(cardW - 30, 22), BackColor = Color.Transparent };
            ingHeader.Controls.Add(new Label { Text = "食材名稱", AutoSize = true, Font = new Font("微軟正黑體", 9f), ForeColor = Color.FromArgb(130, 130, 130), Location = new Point(0, 2) });
            ingHeader.Controls.Add(new Label { Text = "用量", AutoSize = true, Font = new Font("微軟正黑體", 9f), ForeColor = Color.FromArgb(130, 130, 130), Location = new Point(340, 2) });
            cardIng.Controls.Add(ingHeader);

            pnlIngredients = new Panel
            {
                Location = new Point(12, 66),
                Size = new Size(cardW - 30, 150),
                BackColor = Color.Transparent,
                AutoScroll = false
            };
            cardIng.Controls.Add(pnlIngredients);

            var btnAddIng = CreateBtn("＋ 新增食材", 12, 222, 130, 30);
            btnAddIng.FlatAppearance.BorderColor = Color.FromArgb(150, 150, 150);
            btnAddIng.Font = new Font("微軟正黑體", 9.5f);
            btnAddIng.Click += (s, e) => AddIngredientRow("", "");
            cardIng.Controls.Add(btnAddIng);

            AddIngredientRow("", "");
            AddIngredientRow("", "");

            // ---- 評分卡 ----
            var cardRating = MakeCard(cx, 512, 340, 150, "評分與狀態");
            cardRating.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            scroll.Controls.Add(cardRating);

            MakeLabel(cardRating, "我的評分", 12, 44);
            var starPanel = new Panel { Location = new Point(12, 64), Size = new Size(260, 36), BackColor = Color.Transparent };
            cardRating.Controls.Add(starPanel);

            for (int i = 0; i < 5; i++)
            {
                int idx = i;
                var star = new Button
                {
                    Text = "★",
                    Location = new Point(i * 48, 0),
                    Size = new Size(44, 36),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent,
                    ForeColor = Color.FromArgb(200, 200, 200),
                    Font = new Font("微軟正黑體", 18f),
                    Cursor = Cursors.Hand
                };
                star.FlatAppearance.BorderSize = 0;
                star.FlatAppearance.MouseOverBackColor = Color.Transparent;
                star.Click += (s, e) => SetRating((idx + 1) * 2);
                starPanel.Controls.Add(star);
                _stars[i] = star;
            }

            lblRatingVal = new Label
            {
                Text = "0 / 10",
                AutoSize = true,
                Font = new Font("微軟正黑體", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                Location = new Point(252, 68)
            };
            cardRating.Controls.Add(lblRatingVal);

            chkIsMade = new CheckBox
            {
                Text = "已做過這道料理",
                AutoSize = true,
                Font = new Font("微軟正黑體", 10f),
                ForeColor = Color.FromArgb(40, 40, 40),
                Location = new Point(12, 112)
            };
            cardRating.Controls.Add(chkIsMade);

            // ---- 備註卡 ----
            var cardNote = MakeCard(340 + 16, 512, cardW - 340 - 16, 150, "備註");
            cardNote.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            scroll.Controls.Add(cardNote);

            txtNote = new TextBox
            {
                Location = new Point(12, 44),
                Size = new Size(cardNote.Width - 30, 90),
                Font = new Font("微軟正黑體", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            cardNote.Controls.Add(txtNote);

            // ---- 步驟卡 ----
            var cardSteps = MakeCard(cx, 678, cardW, 220, "烹飪步驟");
            cardSteps.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            scroll.Controls.Add(cardSteps);

            txtSteps = new TextBox
            {
                Location = new Point(12, 44),
                Size = new Size(cardW - 30, 158),
                Font = new Font("微軟正黑體", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            cardSteps.Controls.Add(txtSteps);

            pnlBottom.BringToFront();
            pnlTop.BringToFront();
        }

        private void AddIngredientRow(string name, string amount)
        {
            int y = (pnlIngredients.Controls.Count / 1) * 36;
            if (y + 34 > pnlIngredients.Height)
            {
                pnlIngredients.Height = y + 40;
                var card = pnlIngredients.Parent as Panel;
                if (card != null) card.Height = card.Height + 36;
            }

            var row = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(pnlIngredients.Width, 32),
                BackColor = Color.Transparent,
                Tag = "row"
            };

            var txtIName = new TextBox
            {
                Location = new Point(0, 0),
                Size = new Size(320, 30),
                Font = new Font("微軟正黑體", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                Text = name
            };
            row.Controls.Add(txtIName);

            var txtIAmt = new TextBox
            {
                Location = new Point(328, 0),
                Size = new Size(150, 30),
                Font = new Font("微軟正黑體", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                Text = amount
            };
            row.Controls.Add(txtIAmt);

            var btnDel = new Button
            {
                Text = "✕",
                Location = new Point(486, 0),
                Size = new Size(30, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(163, 45, 45),
                Font = new Font("微軟正黑體", 10f),
                Cursor = Cursors.Hand
            };
            btnDel.FlatAppearance.BorderColor = Color.FromArgb(240, 149, 149);
            btnDel.Click += (s, e) =>
            {
                pnlIngredients.Controls.Remove(row);
                ReorderIngRows();
            };
            row.Controls.Add(btnDel);

            pnlIngredients.Controls.Add(row);
        }

        private void ReorderIngRows()
        {
            int y = 0;
            foreach (Control c in pnlIngredients.Controls)
            {
                c.Location = new Point(0, y);
                y += 36;
            }
        }

        private void SetRating(int val)
        {
            _rating = val;
            for (int i = 0; i < 5; i++)
                _stars[i].ForeColor = ((i + 1) * 2 <= val)
                    ? Color.FromArgb(186, 117, 23)
                    : Color.FromArgb(200, 200, 200);
            lblRatingVal.Text = val + " / 10";
        }

        private void LoadData()
        {
            txtName.Text = _recipe.Name;
            cboCuisine.Text = _recipe.CuisineType;
            cboMeal.Text = _recipe.MealType;
            cboDifficulty.Text = _recipe.Difficulty;
            txtCookTime.Text = _recipe.CookTime > 0 ? _recipe.CookTime.ToString() : "";
            txtServings.Text = _recipe.Servings > 0 ? _recipe.Servings.ToString() : "";
            txtSteps.Text = _recipe.Steps;
            txtNote.Text = _recipe.Note;
            chkIsMade.Checked = _recipe.IsMade;
            SetRating(_recipe.Rating);

            pnlIngredients.Controls.Clear();
            foreach (var ing in _recipe.Ingredients)
                AddIngredientRow(ing.Name, ing.Amount);
            if (_recipe.Ingredients.Count == 0)
            {
                AddIngredientRow("", "");
                AddIngredientRow("", "");
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("請輸入料理名稱！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var recipe = new RecipeItem
            {
                RecipeID    = _isEdit ? _recipe.RecipeID : 0,
                Name        = txtName.Text.Trim(),
                CuisineType = cboCuisine.Text,
                MealType    = cboMeal.Text,
                Difficulty  = cboDifficulty.Text,
                CookTime    = int.TryParse(txtCookTime.Text, out int ct) ? ct : 0,
                Servings    = int.TryParse(txtServings.Text, out int sv) ? sv : 0,
                Steps       = txtSteps.Text,
                Note        = txtNote.Text,
                IsMade      = chkIsMade.Checked,
                Rating      = _rating,
                Ingredients = new List<IngredientItem>()
            };

            foreach (Control c in pnlIngredients.Controls)
            {
                if (c.Tag?.ToString() != "row") continue;
                string iname = "", iamt = "";
                foreach (Control cc in c.Controls)
                {
                    if (cc is TextBox tb)
                    {
                        if (tb.Location.X == 0) iname = tb.Text.Trim();
                        else iamt = tb.Text.Trim();
                    }
                }
                if (!string.IsNullOrEmpty(iname))
                    recipe.Ingredients.Add(new IngredientItem { Name = iname, Amount = iamt });
            }

            bool ok = _isEdit ? DBHelper.UpdateRecipe(recipe) : DBHelper.AddRecipe(recipe);
            if (ok)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                string detail = string.IsNullOrEmpty(DBHelper.LastError) ? "" : "\n\n錯誤詳情：\n" + DBHelper.LastError;
                MessageBox.Show("儲存失敗，請確認資料庫連線。" + detail, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===== 輔助方法 =====
        private Panel MakeCard(int x, int y, int w, int h, string title)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = Color.White
            };
            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using (var pen = new Pen(Color.FromArgb(20, 0, 0, 0)))
                    g.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            card.Controls.Add(new Label
            {
                Text = title,
                AutoSize = true,
                Font = new Font("微軟正黑體", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 120, 120),
                Location = new Point(12, 14)
            });

            var sep = new Panel
            {
                Location = new Point(0, 36),
                Size = new Size(w, 1),
                BackColor = Color.FromArgb(20, 0, 0, 0)
            };
            card.Controls.Add(sep);
            return card;
        }

        private TextBox MakeTextBox(Panel parent, string label, int x, int y, int w, int h)
        {
            parent.Controls.Add(new Label
            {
                Text = label,
                AutoSize = true,
                Font = new Font("微軟正黑體", 9.5f),
                ForeColor = Color.FromArgb(100, 100, 100),
                Location = new Point(x, y - 20)
            });
            var tb = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                Font = new Font("微軟正黑體", 10.5f),
                BorderStyle = BorderStyle.FixedSingle
            };
            parent.Controls.Add(tb);
            return tb;
        }

        private Label MakeLabel(Panel parent, string text, int x, int y)
        {
            var lbl = new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font("微軟正黑體", 9.5f),
                ForeColor = Color.FromArgb(100, 100, 100),
                Location = new Point(x, y)
            };
            parent.Controls.Add(lbl);
            return lbl;
        }

        private ComboBox MakeCombo(Panel parent, string[] items, int x, int y, int w)
        {
            var cbo = new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(w, 30),
                Font = new Font("微軟正黑體", 10f),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbo.Items.AddRange(items);
            cbo.SelectedIndex = 0;
            parent.Controls.Add(cbo);
            return cbo;
        }

        private Button CreateBtn(string text, int x, int y, int w, int h)
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
