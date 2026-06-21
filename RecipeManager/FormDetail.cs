using System;
using System.Drawing;
using System.Windows.Forms;

namespace RecipeManager
{
    public partial class FormDetail : Form
    {
        private RecipeItem _recipe;

        public FormDetail(RecipeItem recipe)
        {
            InitializeComponent();
            _recipe = recipe;
            BuildUI();
        }

        private void BuildUI()
        {
            // 清理機制優化：清除舊控制項以供編輯後安全重繪
            this.Controls.Clear();

            this.Text = "詳細食譜";
            this.Size = new Size(760, 680);
            this.MinimumSize = new Size(660, 560);
            this.BackColor = Color.FromArgb(245, 244, 241);
            this.Font = new Font("微軟正黑體", 10f);
            this.StartPosition = FormStartPosition.CenterParent;

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
            btnBack.Click += (s, e) => this.Close();
            pnlTop.Controls.Add(btnBack);

            pnlTop.Controls.Add(new Label
            {
                Text = "食譜詳細",
                Font = new Font("微軟正黑體", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize = true,
                Location = new Point(58, 16)
            });

            var btnEdit = CreateBtn("✏  編輯", 0, 12, 86, 32);
            btnEdit.BackColor = Color.FromArgb(238, 237, 254);
            btnEdit.ForeColor = Color.FromArgb(60, 52, 137);
            btnEdit.FlatAppearance.BorderColor = Color.FromArgb(83, 74, 183);
            btnEdit.Click += (s, e) =>
            {
                var form = new FormEdit(_recipe);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _recipe = DBHelper.GetRecipeByID(_recipe.RecipeID);
                    BuildUI(); // 重新整理乾淨的 UI
                }
            };
            pnlTop.Controls.Add(btnEdit);

            var btnDel = CreateBtn("🗑  刪除", 0, 12, 86, 32);
            btnDel.ForeColor = Color.FromArgb(163, 45, 45);
            btnDel.FlatAppearance.BorderColor = Color.FromArgb(240, 149, 149);
            btnDel.Click += (s, e) =>
            {
                var r = MessageBox.Show("確定要刪除這道食譜嗎？", "確認刪除",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r == DialogResult.Yes)
                {
                    DBHelper.DeleteRecipe(_recipe.RecipeID);
                    this.Close();
                }
            };
            pnlTop.Controls.Add(btnDel);

            pnlTop.Resize += (s, e) =>
            {
                btnDel.Location = new Point(pnlTop.Width - 16 - 86, 12);
                btnEdit.Location = new Point(pnlTop.Width - 16 - 86 - 96, 12);
            };

            // ===== Hero 區塊 =====
            var pnlHero = new Panel
            {
                Dock = DockStyle.Top,
                Height = 140,
                BackColor = Color.White
            };
            pnlHero.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Color.FromArgb(30, 0, 0, 0)),
                    0, pnlHero.Height - 1, pnlHero.Width, pnlHero.Height - 1);

            pnlHero.Controls.Add(new Label
            {
                Text = _recipe.Name,
                Font = new Font("微軟正黑體", 16f, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 20, 20),
                AutoSize = true,
                Location = new Point(24, 14)
            });

            // Badges
            int bx = 24;
            if (!string.IsNullOrEmpty(_recipe.CuisineType))
                bx += AddBadge(pnlHero, _recipe.CuisineType, bx, 52,
                    Color.FromArgb(238, 237, 254), Color.FromArgb(60, 52, 137)) + 8;
            if (!string.IsNullOrEmpty(_recipe.MealType))
                bx += AddBadge(pnlHero, _recipe.MealType, bx, 52,
                    Color.FromArgb(225, 245, 238), Color.FromArgb(8, 80, 65)) + 8;
            AddBadge(pnlHero, _recipe.IsMade ? "✓ 做過" : "未做過", bx, 52,
                _recipe.IsMade ? Color.FromArgb(234, 243, 222) : Color.FromArgb(241, 239, 232),
                _recipe.IsMade ? Color.FromArgb(39, 80, 10) : Color.FromArgb(95, 94, 90));

            // Stat Cards
            int sx = 24;
            int sy = 82;
            int sw = 140, sh = 46;
            AddStatCard(pnlHero, sx, sy, sw, sh, "烹飪時間", _recipe.CookTime > 0 ? _recipe.CookTime + " 分鐘" : "—"); sx += sw + 10;
            AddStatCard(pnlHero, sx, sy, sw, sh, "份量", _recipe.Servings > 0 ? _recipe.Servings + " 人份" : "—"); sx += sw + 10;
            AddStatCard(pnlHero, sx, sy, sw, sh, "難度", string.IsNullOrEmpty(_recipe.Difficulty) ? "—" : _recipe.Difficulty); sx += sw + 10;
            AddStatCard(pnlHero, sx, sy, sw, sh, "評分", _recipe.Rating > 0 ? "★ " + _recipe.Rating + " / 10" : "—");

            // ===== 捲動主體 =====
            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 244, 241),
                Padding = new Padding(24, 20, 24, 40)
            };

            // 控制項加入先後順序（防 Fill 重疊遮擋）
            this.Controls.Add(scroll);
            this.Controls.Add(pnlHero);
            this.Controls.Add(pnlTop);

            int cardW = 690;

            // ---- 食材清單卡（左側固定寬度） ----
            var cardIng = MakeCard(scroll, 0, 0, 320, 0, "食材清單");
            int iy = 44;
            foreach (var ing in _recipe.Ingredients)
            {
                var row = new Panel
                {
                    Location = new Point(0, iy),
                    Size = new Size(cardIng.Width, 32),
                    BackColor = Color.Transparent
                };
                row.Paint += (s, e) =>
                    e.Graphics.DrawLine(new Pen(Color.FromArgb(15, 0, 0, 0)),
                        12, row.Height - 1, row.Width - 12, row.Height - 1);

                var dot = new Panel
                {
                    Location = new Point(12, 12),
                    Size = new Size(6, 6),
                    BackColor = Color.FromArgb(83, 74, 183)
                };
                row.Controls.Add(dot);

                row.Controls.Add(new Label
                {
                    Text = ing.Name,
                    AutoSize = true,
                    Font = new Font("微軟正黑體", 10f),
                    ForeColor = Color.FromArgb(30, 30, 30),
                    Location = new Point(24, 6)
                });
                row.Controls.Add(new Label
                {
                    Text = ing.Amount,
                    AutoSize = true,
                    Font = new Font("微軟正黑體", 10f),
                    ForeColor = Color.FromArgb(120, 120, 120),
                    Location = new Point(220, 6)
                });
                cardIng.Controls.Add(row);
                iy += 32;
            }
            cardIng.Height = Math.Max(iy + 16, 140);

            // 💡【排版調整修正核心】：移除大評分卡，讓右側只放備註卡並徹底置頂
            int rightX = 320 + 16;
            int noteY = 0; // 起點重置為 0，與左側食材清單水平對齊

            if (!string.IsNullOrEmpty(_recipe.Note))
            {
                var cardNote = MakeCard(scroll, rightX, noteY, cardW - 320 - 16, 0, "備註");
                cardNote.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; // 支援拉寬縮放
                var lblNote = new Label
                {
                    Text = _recipe.Note,
                    Font = new Font("微軟正黑體", 10f),
                    ForeColor = Color.FromArgb(80, 80, 80),
                    Location = new Point(12, 44),
                    MaximumSize = new Size(cardNote.Width - 24, 0),
                    AutoSize = true
                };
                cardNote.Controls.Add(lblNote);
                cardNote.Height = lblNote.Top + lblNote.Height + 16;
                noteY += cardNote.Height + 16;
            }

            // ---- 步驟卡 ----
            // 步驟卡頂部自動緊貼在「食材卡高」或「備註卡高」中最長的那一側下方
            int stepsTop = Math.Max(cardIng.Height + 20, noteY);
            var cardSteps = MakeCard(scroll, 0, stepsTop, cardW, 0, "烹飪步驟");
            cardSteps.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            int stepY = 44;
            var lines = _recipe.Steps?.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
            int stepNum = 1;
            foreach (var line in lines)
            {
                var numCircle = new Panel
                {
                    Location = new Point(12, stepY + 2),
                    Size = new Size(26, 26),
                    BackColor = Color.FromArgb(238, 237, 254)
                };
                numCircle.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(238, 237, 254)),
                        0, 0, 25, 25);
                };
                var numLbl = new Label
                {
                    Text = stepNum.ToString(),
                    AutoSize = false,
                    Size = new Size(26, 26),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("微軟正黑體", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(60, 52, 137),
                    BackColor = Color.Transparent
                };
                numCircle.Controls.Add(numLbl);
                cardSteps.Controls.Add(numCircle);

                var stepLbl = new Label
                {
                    Text = line.Trim(),
                    Font = new Font("微軟正黑體", 10f),
                    ForeColor = Color.FromArgb(30, 30, 30),
                    Location = new Point(48, stepY),
                    MaximumSize = new Size(cardW - 72, 0),
                    AutoSize = true
                };
                cardSteps.Controls.Add(stepLbl);

                var sepLine = new Panel
                {
                    Location = new Point(12, stepY + stepLbl.Height + 10),
                    Size = new Size(cardW - 30, 1),
                    BackColor = Color.FromArgb(15, 0, 0, 0),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                cardSteps.Controls.Add(sepLine);

                stepY += stepLbl.Height + 26;
                stepNum++;
            }
            if (lines.Length == 0)
            {
                cardSteps.Controls.Add(new Label
                {
                    Text = "尚未填寫烹飪步驟",
                    AutoSize = true,
                    Font = new Font("微軟正黑體", 10f),
                    ForeColor = Color.FromArgb(150, 150, 150),
                    Location = new Point(12, 44)
                });
                stepY = 80;
            }
            cardSteps.Height = stepY + 16;
        }

        private int AddBadge(Panel parent, string text, int x, int y, Color bg, Color fg)
        {
            var lbl = new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font("微軟正黑體", 9.5f, FontStyle.Bold),
                ForeColor = fg,
                BackColor = bg,
                Location = new Point(x, y),
                Padding = new Padding(8, 3, 8, 3)
            };
            parent.Controls.Add(lbl);
            return lbl.Width;
        }

        private void AddStatCard(Panel parent, int x, int y, int w, int h, string label, string value)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = Color.FromArgb(245, 244, 241)
            };
            card.Controls.Add(new Label
            {
                Text = label,
                AutoSize = true,
                Font = new Font("微軟正黑體", 8.5f),
                ForeColor = Color.FromArgb(130, 130, 130),
                Location = new Point(10, 4)
            });
            card.Controls.Add(new Label
            {
                Text = value,
                AutoSize = true,
                Font = new Font("微軟正黑體", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(10, 22)
            });
            parent.Controls.Add(card);
        }

        private Panel MakeCard(Panel parent, int x, int y, int w, int h, string title)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h == 0 ? 120 : h),
                BackColor = Color.White
            };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(20, 0, 0, 0)))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };
            card.Controls.Add(new Label
            {
                Text = title,
                AutoSize = true,
                Font = new Font("微軟正黑體", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 120, 120),
                Location = new Point(12, 14)
            });

            var line = new Panel
            {
                Location = new Point(0, 36),
                Size = new Size(w, 1),
                BackColor = Color.FromArgb(20, 0, 0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Controls.Add(line);

            parent.Controls.Add(card);
            return card;
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