using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RecipeManager
{
    public class RecipeItem
    {
        public int RecipeID { get; set; }
        public string Name { get; set; }
        public string CuisineType { get; set; }
        public string MealType { get; set; }
        public string Difficulty { get; set; }
        public int CookTime { get; set; }
        public int Servings { get; set; }
        public string Steps { get; set; }
        public string Note { get; set; }
        public bool IsMade { get; set; }
        public int Rating { get; set; }
        public List<IngredientItem> Ingredients { get; set; } = new List<IngredientItem>();
    }

    public class IngredientItem
    {
        public int IngredientID { get; set; }
        public string Name { get; set; }
        public string Amount { get; set; }
    }

    public static class DBHelper
    {
        public static string LastError { get; private set; } = "";

        private static readonly string ConnStr =
            @"Data Source=(LocalDB)\MSSQLLocalDB;" +
            @"AttachDBFilename=" + System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                "Database1.mdf") + ";" +
            @"Integrated Security=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnStr);
        }

        public static List<RecipeItem> GetRecipes(string nameFilter = "",
            string cuisineType = "", string mealType = "",
            string difficulty = "", string cookTimeRange = "", string isMade = "")
        {
            var list = new List<RecipeItem>();
            var conditions = new List<string>();

            if (!string.IsNullOrEmpty(nameFilter))
                conditions.Add("Name LIKE @Name");
            if (!string.IsNullOrEmpty(cuisineType))
                conditions.Add("CuisineType = @CuisineType");
            if (!string.IsNullOrEmpty(mealType))
                conditions.Add("MealType = @MealType");
            if (!string.IsNullOrEmpty(difficulty))
                conditions.Add("Difficulty = @Difficulty");
            if (cookTimeRange == "15以內")
                conditions.Add("CookTime <= 15");
            else if (cookTimeRange == "15-30")
                conditions.Add("CookTime > 15 AND CookTime <= 30");
            else if (cookTimeRange == "30-60")
                conditions.Add("CookTime > 30 AND CookTime <= 60");
            else if (cookTimeRange == "60以上")
                conditions.Add("CookTime > 60");
            if (isMade == "做過")
                conditions.Add("IsMade = 1");
            else if (isMade == "未做過")
                conditions.Add("IsMade = 0");

            var sql = "SELECT * FROM Recipes WHERE 1=1";
            if (conditions.Count > 0)
                sql += " AND " + string.Join(" AND ", conditions);
            sql += " ORDER BY RecipeID DESC";

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (!string.IsNullOrEmpty(nameFilter))
                        cmd.Parameters.AddWithValue("@Name", "%" + nameFilter + "%");
                    if (!string.IsNullOrEmpty(cuisineType))
                        cmd.Parameters.AddWithValue("@CuisineType", cuisineType);
                    if (!string.IsNullOrEmpty(mealType))
                        cmd.Parameters.AddWithValue("@MealType", mealType);
                    if (!string.IsNullOrEmpty(difficulty))
                        cmd.Parameters.AddWithValue("@Difficulty", difficulty);

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new RecipeItem
                            {
                                RecipeID = dr.GetInt32(0),
                                Name = dr["Name"].ToString().Trim(),
                                CuisineType = dr["CuisineType"].ToString().Trim(),
                                MealType = dr["MealType"].ToString().Trim(),
                                Difficulty = dr["Difficulty"].ToString().Trim(),
                                CookTime = dr["CookTime"] == DBNull.Value ? 0 : (int)dr["CookTime"],
                                Servings = dr["Servings"] == DBNull.Value ? 0 : (int)dr["Servings"],
                                Steps = dr["Steps"].ToString(),
                                Note = dr["Note"].ToString(),
                                IsMade = dr["IsMade"] != DBNull.Value && (bool)dr["IsMade"],
                                Rating = dr["Rating"] == DBNull.Value ? 0 : (int)dr["Rating"]
                            });
                        }
                    }
                }
            }
            return list;
        }

        public static RecipeItem GetRecipeByID(int id)
        {
            RecipeItem recipe = null;
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT * FROM Recipes WHERE RecipeID = @ID", conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            recipe = new RecipeItem
                            {
                                RecipeID = dr.GetInt32(0),
                                Name = dr["Name"].ToString().Trim(),
                                CuisineType = dr["CuisineType"].ToString().Trim(),
                                MealType = dr["MealType"].ToString().Trim(),
                                Difficulty = dr["Difficulty"].ToString().Trim(),
                                CookTime = dr["CookTime"] == DBNull.Value ? 0 : (int)dr["CookTime"],
                                Servings = dr["Servings"] == DBNull.Value ? 0 : (int)dr["Servings"],
                                Steps = dr["Steps"].ToString(),
                                Note = dr["Note"].ToString(),
                                IsMade = dr["IsMade"] != DBNull.Value && (bool)dr["IsMade"],
                                Rating = dr["Rating"] == DBNull.Value ? 0 : (int)dr["Rating"]
                            };
                        }
                    }
                }

                if (recipe != null)
                {
                    var sql2 = @"SELECT i.IngredientID, i.Name, ri.Amount
                                 FROM RecipeIngredients ri
                                 INNER JOIN Ingredients i ON ri.IngredientID = i.IngredientID
                                 WHERE ri.RecipeID = @ID";
                    using (var cmd2 = new SqlCommand(sql2, conn))
                    {
                        cmd2.Parameters.AddWithValue("@ID", id);
                        using (var dr2 = cmd2.ExecuteReader())
                        {
                            while (dr2.Read())
                            {
                                recipe.Ingredients.Add(new IngredientItem
                                {
                                    IngredientID = (int)dr2["IngredientID"],
                                    Name = dr2["Name"].ToString().Trim(),
                                    Amount = dr2["Amount"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            return recipe;
        }

        public static bool AddRecipe(RecipeItem recipe)
        {
            LastError = "";
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            var sql = @"INSERT INTO Recipes
                                (Name,CuisineType,MealType,Difficulty,CookTime,Servings,Steps,Note,IsMade,Rating)
                                OUTPUT INSERTED.RecipeID
                                VALUES (@Name,@CuisineType,@MealType,@Difficulty,@CookTime,@Servings,@Steps,@Note,@IsMade,@Rating)";
                            int newID;
                            using (var cmd = new SqlCommand(sql, conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@Name", recipe.Name);
                                cmd.Parameters.AddWithValue("@CuisineType", recipe.CuisineType ?? "");
                                cmd.Parameters.AddWithValue("@MealType", recipe.MealType ?? "");
                                cmd.Parameters.AddWithValue("@Difficulty", recipe.Difficulty ?? "");
                                cmd.Parameters.AddWithValue("@CookTime", recipe.CookTime);
                                cmd.Parameters.AddWithValue("@Servings", recipe.Servings);
                                cmd.Parameters.AddWithValue("@Steps", recipe.Steps ?? "");
                                cmd.Parameters.AddWithValue("@Note", recipe.Note ?? "");
                                cmd.Parameters.AddWithValue("@IsMade", recipe.IsMade);
                                cmd.Parameters.AddWithValue("@Rating", recipe.Rating);
                                newID = (int)cmd.ExecuteScalar();
                            }
                            SaveIngredients(conn, tran, newID, recipe.Ingredients);
                            tran.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            LastError = ex.Message;
                            tran.Rollback();
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

        public static bool UpdateRecipe(RecipeItem recipe)
        {
            LastError = "";
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            var sql = @"UPDATE Recipes SET
                                Name=@Name, CuisineType=@CuisineType, MealType=@MealType,
                                Difficulty=@Difficulty, CookTime=@CookTime, Servings=@Servings,
                                Steps=@Steps, Note=@Note, IsMade=@IsMade, Rating=@Rating
                                WHERE RecipeID=@ID";
                            using (var cmd = new SqlCommand(sql, conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@Name", recipe.Name);
                                cmd.Parameters.AddWithValue("@CuisineType", recipe.CuisineType ?? "");
                                cmd.Parameters.AddWithValue("@MealType", recipe.MealType ?? "");
                                cmd.Parameters.AddWithValue("@Difficulty", recipe.Difficulty ?? "");
                                cmd.Parameters.AddWithValue("@CookTime", recipe.CookTime);
                                cmd.Parameters.AddWithValue("@Servings", recipe.Servings);
                                cmd.Parameters.AddWithValue("@Steps", recipe.Steps ?? "");
                                cmd.Parameters.AddWithValue("@Note", recipe.Note ?? "");
                                cmd.Parameters.AddWithValue("@IsMade", recipe.IsMade);
                                cmd.Parameters.AddWithValue("@Rating", recipe.Rating);
                                cmd.Parameters.AddWithValue("@ID", recipe.RecipeID);
                                cmd.ExecuteNonQuery();
                            }
                            using (var cmd2 = new SqlCommand(
                                "DELETE FROM RecipeIngredients WHERE RecipeID=@ID", conn, tran))
                            {
                                cmd2.Parameters.AddWithValue("@ID", recipe.RecipeID);
                                cmd2.ExecuteNonQuery();
                            }
                            SaveIngredients(conn, tran, recipe.RecipeID, recipe.Ingredients);
                            tran.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            LastError = ex.Message;
                            tran.Rollback();
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

        public static bool DeleteRecipe(int id)
        {
            LastError = "";
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            using (var cmd = new SqlCommand(
                                "DELETE FROM RecipeIngredients WHERE RecipeID=@ID", conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@ID", id);
                                cmd.ExecuteNonQuery();
                            }
                            using (var cmd2 = new SqlCommand(
                                "DELETE FROM Recipes WHERE RecipeID=@ID", conn, tran))
                            {
                                cmd2.Parameters.AddWithValue("@ID", id);
                                cmd2.ExecuteNonQuery();
                            }
                            tran.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            LastError = ex.Message;
                            tran.Rollback();
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

        private static void SaveIngredients(SqlConnection conn, SqlTransaction tran,
            int recipeID, List<IngredientItem> ingredients)
        {
            var addedIngIDs = new HashSet<int>();
            foreach (var ing in ingredients)
            {
                if (string.IsNullOrWhiteSpace(ing.Name)) continue;
                int ingID;
                using (var cmd = new SqlCommand(
                    "SELECT IngredientID FROM Ingredients WHERE Name=@Name", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@Name", ing.Name.Trim());
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        ingID = (int)result;
                    }
                    else
                    {
                        using (var insCmd = new SqlCommand(
                            "INSERT INTO Ingredients(Name) OUTPUT INSERTED.IngredientID VALUES(@Name)",
                            conn, tran))
                        {
                            insCmd.Parameters.AddWithValue("@Name", ing.Name.Trim());
                            ingID = (int)insCmd.ExecuteScalar();
                        }
                    }
                }
                if (addedIngIDs.Contains(ingID)) continue;
                addedIngIDs.Add(ingID);
                using (var riCmd = new SqlCommand(
                    "INSERT INTO RecipeIngredients(RecipeID,IngredientID,Amount) VALUES(@RID,@IID,@Amt)",
                    conn, tran))
                {
                    riCmd.Parameters.AddWithValue("@RID", recipeID);
                    riCmd.Parameters.AddWithValue("@IID", ingID);
                    riCmd.Parameters.AddWithValue("@Amt", ing.Amount ?? "");
                    riCmd.ExecuteNonQuery();
                }
            }
        }
    }
}
