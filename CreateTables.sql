-- 請在 Visual Studio 伺服器總管建立 recipe.mdf 後，
-- 依序執行以下 SQL 建立三張資料表

-- 1. 食譜主表
CREATE TABLE [dbo].[Recipes] (
    [RecipeID]    INT            IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name]        NVARCHAR(100)  NOT NULL,
    [CuisineType] NCHAR(10)      NULL,
    [MealType]    NCHAR(10)      NULL,
    [Difficulty]  NCHAR(10)      NULL,
    [CookTime]    INT            NULL,
    [Servings]    INT            NULL,
    [Steps]       NVARCHAR(2000) NULL,
    [Note]        NVARCHAR(500)  NULL,
    [IsMade]      BIT            DEFAULT(0) NULL,
    [Rating]      INT            DEFAULT(0) NULL
);

-- 2. 食材表
CREATE TABLE [dbo].[Ingredients] (
    [IngredientID] INT           IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name]         NVARCHAR(50)  NOT NULL
);

-- 3. 食譜食材關聯表
CREATE TABLE [dbo].[RecipeIngredients] (
    [RecipeID]     INT           NOT NULL,
    [IngredientID] INT           NOT NULL,
    [Amount]       NVARCHAR(50)  NULL,
    PRIMARY KEY ([RecipeID], [IngredientID])
);
