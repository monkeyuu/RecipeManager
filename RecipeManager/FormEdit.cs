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
            this.Size = new Size(780, 800);
            this.MinimumSize = new Size(680, 580);
            this.BackColor = Color.FromArgb(245, 244, 241);
            this.Font = new Font("微軟正黑體", 10f);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // ===== 頂部 =====
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
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
                Size = new Size(30, 30),
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
                Height = 50,
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
                // 💡 修正 1：不要用 DockStyle.Fill 了，改成 None 讓我們自己控制位置！
                Dock = DockStyle.None,
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 244, 241),
                // 既然主體邊界已經被完全頂開，內部 Padding 恢復正常的安全距離即可
                Padding = new Padding(24, 20, 24, 20),

                // 💡 修正 2：手動指定起點，剛好接在頂部面板（高度 50）的正下方
                Location = new Point(0, 50),

                // 💡 修正 3：手動計算寬高
                // 寬度 = 視窗扣掉左右邊框寬度 (約 16)；高度 = 視窗高度扣掉頂部(50)、底部(50)與上下邊框 (約 40)
                Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 100)
            };
            var scroll = _scroll;
            this.Controls.Add(scroll);

            // 💡 修正 4：為了防止使用者「拉大或縮小視窗」時高度又跑掉，加上 Anchor 讓它上下左右跟著視窗同步縮放！
            scroll.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // 💡 修正 5：保險起見，把底部和頂部面板帶到最上層，確保它們牢牢黏在最外圍
            _pnlBottom.BringToFront();
            pnlTop.BringToFront();

            int cardW = 700, cx = 0;

            // ---- 基本資訊卡 ----
            var cardBasic = MakeCard(cx, 15, cardW, 220, "基本資訊");
            cardBasic.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            scroll.Controls.Add(cardBasic);

            txtName = MakeTextBox(cardBasic, "料理名稱", 12, 65, cardW - 30, 34);

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
            var cardIng = MakeCard(cx, 250, cardW, 265, "食材清單"); // 固定高度 265
            cardIng.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            scroll.Controls.Add(cardIng);

            var ingHeader = new Panel { Location = new Point(12, 42), Size = new Size(cardW - 30, 22), BackColor = Color.Transparent };
            ingHeader.Controls.Add(new Label { Text = "食材名稱", AutoSize = true, Font = new Font("微軟正黑體", 9f), ForeColor = Color.FromArgb(130, 130, 130), Location = new Point(0, 2) });
            ingHeader.Controls.Add(new Label { Text = "用量", AutoSize = true, Font = new Font("微軟正黑體", 9f), ForeColor = Color.FromArgb(130, 130, 130), Location = new Point(340, 2) });
            cardIng.Controls.Add(ingHeader);

            pnlIngredients = new Panel
            {
                Location = new Point(12, 66),
                Size = new Size(cardW - 30, 150), // 嚴格卡死高度 150 不准自己長高
                BackColor = Color.Transparent,
                AutoScroll = true // 💡 保持開啟，現在它終於能派上用場了！
            };
            cardIng.Controls.Add(pnlIngredients);

            // 💡 修正按鈕 Y 座標為 225，這是在 card 內部（高265）最安全的精準位置
            var btnAddIng = CreateBtn("＋ 新增食材", 12, 225, 130, 30);
            btnAddIng.FlatAppearance.BorderColor = Color.FromArgb(150, 150, 150);
            btnAddIng.Font = new Font("微軟正黑體", 9.5f);
            btnAddIng.Click += (s, e) => AddIngredientRow("", "");
            cardIng.Controls.Add(btnAddIng);

            AddIngredientRow("", "");
            AddIngredientRow("", "");

            // ---- 評分卡 ----
            var cardRating = MakeCard(cx, 525, 340, 160, "評分與狀態"); // 修正：Y 從 512 移到 540，高度改 160 容納核取方塊
            cardRating.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            scroll.Controls.Add(cardRating);

            MakeLabel(cardRating, "我的評分", 12, 44);
            var starPanel = new Panel { Location = new Point(12, 64), Size = new Size(240, 36), BackColor = Color.Transparent };
            cardRating.Controls.Add(starPanel);

            for (int i = 0; i < 5; i++)
            {
                int idx = i;
                var star = new Button
                {
                    Text = "★",
                    Location = new Point(i * 44, 0), // 修正：間距微調為 44，防止星星擠出 panel 邊界
                    Size = new Size(40, 36),
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
                Location = new Point(240, 68) // 修正：位置微調配合星星面板
            };
            cardRating.Controls.Add(lblRatingVal);

            chkIsMade = new CheckBox
            {
                Text = "已做過這道料理",
                AutoSize = true,
                Font = new Font("微軟正黑體", 10f),
                ForeColor = Color.FromArgb(40, 40, 40),
                Location = new Point(12, 115) // 修正：稍微下移防重疊
            };
            cardRating.Controls.Add(chkIsMade);

            // ---- 備註卡 ----
            var cardNote = MakeCard(340 + 16, 525, cardW - 340 - 16, 160, "備註"); // 修正：Y 配合評分卡改為 540，高度同步改為 160
            cardNote.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            scroll.Controls.Add(cardNote);

            txtNote = new TextBox
            {
                Location = new Point(12, 44),
                Size = new Size(cardNote.Width - 30, 100), // 修正：高度調到 100 完整利用空間
                Font = new Font("微軟正黑體", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            cardNote.Controls.Add(txtNote);

            // ---- 步驟卡 ----
            var cardSteps = MakeCard(cx, 700, cardW, 250, "烹飪步驟"); // 修正：Y 從 678 下移到 715，完美避開上方並排的兩張卡片
            cardSteps.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            scroll.Controls.Add(cardSteps);

            txtSteps = new TextBox
            {
                Location = new Point(12, 44),
                Size = new Size(cardW - 30, 195),
                Font = new Font("微軟正head體", 10f),
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
            // 1. 動態計算下一列的 Y 座標
            int y = pnlIngredients.Controls.Count * 36;

            // 💡【關鍵修正】：直接刪除原本會讓 pnlIngredients 和 card 長高的 if 區塊！
            // 讓 pnlIngredients 的高度死死固定在原來的 150 像素，超出時它才會被迫彈出垂直滾動軸。

            var row = new Panel
            {
                Location = new Point(0, y),
                // 💡 修正寬度：pnlIngredients.Width - 25，幫右側滾動軸留下 VIP 空位，防止長出難看的水平滾動條
                Size = new Size(pnlIngredients.Width - 25, 32),
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
                Size = new Size(130, 30), // 💡 微調寬度 150 -> 130，避免擠到刪除按鈕
                Font = new Font("微軟正黑體", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                Text = amount
            };
            row.Controls.Add(txtIAmt);

            var btnDel = new Button
            {
                Text = "✕",
                Location = new Point(466, 0), // 💡 座標微調配合用量輸入框
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

            // 💡 通知排版引擎有新元件加入，並自動把最新的食材列捲動到視線內！
            pnlIngredients.PerformLayout();
            pnlIngredients.ScrollControlIntoView(row);
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
