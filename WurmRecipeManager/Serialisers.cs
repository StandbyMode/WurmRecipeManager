using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Collections.ObjectModel;

namespace WurmRecipeManager
{
    public interface RecipeSerialiser
    {
        // Push a recipe into the database as a recipe to taste. Recipes are moved from the station to the archive and vice versa via "ArchiveRecipe".
        // This must also assign a the serialiser ID.
        void SaveRecipe(Recipe recipe);

        void UpdateRecipeName(Recipe recipe);

        // return true if database contains this exact recipe
        bool ContainsRecipe(Recipe recipe);

        // Search for an affinity on a given consumer, returns an empty list if the requested consumer has no such affinity recorded in the database
        ObservableCollection<Recipe> SearchAffinity(String consumer, String affinity);

        // Search for recipes by name
        ObservableCollection<Recipe> SearchName(String name);

        // Get recipes on standby
        ObservableCollection<Recipe> GetRecipesOnStandby();

        // Move recipe from or to the tasting station to or from the archive
        void ArchiveRecipe(Recipe recipe, bool unarchive = false);

        void UpdateAffinities(Recipe recipe);

        // Delete a recipe from the database for good
        void DeleteRecipe(Recipe recipe);

        // Cleanup procedure called before quitting the program
        void Dispose();
    }

    public class SQLiteSerialiser : RecipeSerialiser {

        public static long STATUS_TASTING = 1;
        public static long STATUS_ARCHIVED = 2;

        private SQLiteConnection conn;

        public SQLiteSerialiser(String database)
        {
            bool init = false;
            if (!System.IO.File.Exists(database)) {
                SQLiteConnection.CreateFile(database);
                init = true;
            }

            conn = new SQLiteConnection("Data Source="+database);

            // New file was created, so initialise tables aswell
            if (init)
            {
                conn.Open();

                new SQLiteCommand("CREATE TABLE Recipes (R_ID INTEGER PRIMARY KEY, Name varchar(64), Container varchar(64), Hash INTEGER NOT NULL, Status INTEGER DEFAULT 0)", conn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE Ingredients (R_ID INTEGER NOT NULL, Ingredient varchar(64), FOREIGN KEY (R_ID) References Recipes(R_ID) ON DELETE CASCADE)", conn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE Consumers (R_ID INTEGER NOT NULL, Consumer varchar(64), Affinity varchar(64), FOREIGN KEY (R_ID) References Recipes(R_ID) ON DELETE CASCADE)", conn).ExecuteNonQuery();

                conn.Close();
            }
            //else
            //{
            //    conn.Open();
            //    var comm = new SQLiteCommand("UPDATE Recipes SET Status = 2 WHERE Status = 0", conn);
            //    //comm.Parameters.Add(new SQLiteParameter(":id", 0));
            //    var res = comm.ExecuteNonQuery();
            //    conn.Close();
            //}
        }

        public void SaveRecipe(Recipe recipe)
        {
            conn.Open();

            using (SQLiteCommand comm = new SQLiteCommand("INSERT INTO Recipes(Name,Container,Hash,Status) VALUES (:name, :container, :hash, :status)", conn))
            {
                comm.Parameters.Add(new SQLiteParameter(":name", recipe.Name));
                comm.Parameters.Add(new SQLiteParameter(":container", recipe.Container));
                comm.Parameters.Add(new SQLiteParameter(":hash", recipe.GetHashCode()));
                comm.Parameters.Add(new SQLiteParameter(":status", STATUS_TASTING));
                comm.ExecuteNonQuery();
            }

            using (SQLiteCommand comm = new SQLiteCommand("SELECT last_insert_rowid()", conn))
            {
                object id = null;
                using (var read = comm.ExecuteReader())
                {
                    read.Read();
                    id = read.GetValue(0);
                    recipe.Id = (long)id; // Assign serialiser id
                }
                foreach (Ingredient ing in recipe.Ingredients)
                {
                    using (SQLiteCommand comm_insert_ings = new SQLiteCommand("INSERT INTO Ingredients(R_Id,Ingredient) VALUES (:id, :ing)", conn))
                    {
                        comm_insert_ings.Parameters.Add(new SQLiteParameter(":id", id));
                        comm_insert_ings.Parameters.Add(new SQLiteParameter(":ing", ing.Name));
                        comm_insert_ings.ExecuteNonQuery();
                    }
                }


                // Recipes are now persisted i nthe tastign station already with a flag, they would'n thave affinities assigned yet.
                //foreach (CharacterAffinity aff in recipe.Affinities)
                //{
                //    using (SQLiteCommand comm_insert_affs = new SQLiteCommand("INSERT INTO Consumers(R_Id,Consumer,Affinity) VALUES (:id, :cons, :aff)", conn))
                //    {
                //        comm_insert_affs.Parameters.Add(new SQLiteParameter(":id", id));
                //        comm_insert_affs.Parameters.Add(new SQLiteParameter(":cons", aff.Character));
                //        comm_insert_affs.Parameters.Add(new SQLiteParameter(":aff", aff.Affinity));
                //        comm_insert_affs.ExecuteNonQuery();
                //    }
                //}
            }
            conn.Close();
        }

        public void UpdateRecipeName(Recipe recipe)
        {
            conn.Open();

            using (SQLiteCommand comm = new SQLiteCommand("UPDATE Recipes SET Name = :name WHERE R_ID = :id", conn))
            {
                comm.Parameters.Add(new SQLiteParameter(":id", recipe.Id));
                comm.Parameters.Add(new SQLiteParameter(":name", recipe.Name));
                int ret = comm.ExecuteNonQuery();
            }
            conn.Close();
        }

        public bool ContainsRecipe(Recipe recipe)
        {
            conn.Open();
            bool res = false;
            using (SQLiteCommand comm = new SQLiteCommand("SELECT R_Id FROM Recipes WHERE Hash=:hash", conn))
            {
                comm.Parameters.Add(new SQLiteParameter(":hash", recipe.GetHashCode()));
                using (var read_id = comm.ExecuteReader())
                {

                    read_id.Read();
                    while (read_id.HasRows)
                    {
                        Recipe cmprec = new Recipe();

                        object id = read_id.GetValue(0);
                        using (SQLiteCommand comm_read_rec = new SQLiteCommand("SELECT * FROM Recipes INNER JOIN Ingredients ON Recipes.R_Id = Ingredients.R_Id WHERE Recipes.R_Id = :id", conn))
                        {
                            comm_read_rec.Parameters.Add(new SQLiteParameter(":id", id));
                            using (var fetch = comm_read_rec.ExecuteReader())
                            {
                                bool first = true;
                                fetch.Read();
                                while (fetch.HasRows)
                                {
                                    if (first)
                                    {
                                        cmprec.Name = (string)fetch.GetValue(1);
                                        cmprec.Container = (string)fetch.GetValue(2);
                                        first = false;
                                    }
                                    string ing = (string)fetch.GetValue(6);
                                    cmprec.Ingredients.Add(new Ingredient() { Name = ing });
                                    fetch.Read();
                                }
                            }
                        }
                        if (cmprec.Equals(recipe))
                        {
                            res = true;
                            break;
                        }

                        read_id.Read();
                    }
                }
            }
            conn.Close();
            return res;
        }

        public ObservableCollection<Recipe> SearchAffinity(String consumer, String affinity)
        {
            ObservableCollection<Recipe> res = new ObservableCollection<Recipe>();
            conn.Open();
            using (SQLiteCommand comm = new SQLiteCommand("SELECT R_Id FROM Consumers WHERE Consumer=:consumer AND Affinity=:aff", conn))
            {
                comm.Parameters.Add(new SQLiteParameter(":consumer", consumer));
                comm.Parameters.Add(new SQLiteParameter(":aff", affinity));
                using (var read_id = comm.ExecuteReader())
                {
                    read_id.Read();
                    while (read_id.HasRows)
                    {
                        long id = (long)read_id.GetValue(0);

                        Recipe recipe = GetRecipeByID(id, STATUS_ARCHIVED);

                        if (recipe != null)
                            res.Add(recipe);

                        read_id.Read();
                    }
                }
            }
            conn.Close();
            return res;
        }

        public ObservableCollection<Recipe> SearchName(String name)
        {
            ObservableCollection<Recipe> res = new ObservableCollection<Recipe>();
            conn.Open();
            using (SQLiteCommand comm = new SQLiteCommand("SELECT R_Id FROM Recipes WHERE Name LIKE :name AND Status = :status", conn))
            {
                comm.Parameters.Add(new SQLiteParameter(":status", STATUS_ARCHIVED));
                
                comm.Parameters.Add(new SQLiteParameter(":name", "%"+name+"%"));
                using (var read_id = comm.ExecuteReader())
                {

                    read_id.Read();
                    while (read_id.HasRows)
                    {

                        long id = (long)read_id.GetValue(0);
                        Recipe recipe = GetRecipeByID(id, STATUS_ARCHIVED);
                        if (recipe != null)
                            res.Add(recipe);

                        read_id.Read();
                    }
                }
            }
            conn.Close();
            return res;
        }

        private Recipe GetRecipeByID(long id, long status)
        {
            Recipe recipe = new Recipe();
            List<Ingredient> ingredients = new List<Ingredient>();
            recipe.Id = (long)id;
            using (SQLiteCommand comm_ings = new SQLiteCommand("SELECT * FROM Recipes INNER JOIN Ingredients ON Recipes.R_Id = Ingredients.R_Id WHERE Recipes.R_Id = :id AND Status = :status", conn))
            {
                comm_ings.Parameters.Add(new SQLiteParameter(":id", id));
                comm_ings.Parameters.Add(new SQLiteParameter(":status", status));
                using (var fetch = comm_ings.ExecuteReader())
                {
                    bool first = true;
                    fetch.Read();
                    while (fetch.HasRows)
                    {
                        if (first)
                        {
                            recipe.Name = (string)fetch.GetValue(1);
                            recipe.Container = (string)fetch.GetValue(2);
                            first = false;
                        }
                        string ing = (string)fetch.GetValue(6);
                        recipe.Ingredients.Add(new Ingredient() { Name = ing });
                        fetch.Read();
                    }

                    //If first is still set, no line was read, presumably because the ID didn't have the desired status. In which case we return NULL
                    if (first) return null;

                }
            }

            //ingredients.Sort((i1,i2) => i1.Name.CompareTo(i2.Name));

            //foreach (Ingredient ing in ingredients)
            //    recipe.Ingredients.Add(ing);

            using (SQLiteCommand comm_affs = new SQLiteCommand("SELECT Consumer,Affinity FROM Consumers WHERE R_Id = :id ORDER BY Consumer ASC", conn))
            {
                comm_affs.Parameters.Add(new SQLiteParameter(":id", id));
                using (var fetch = comm_affs.ExecuteReader())
                {
                    fetch.Read();
                    while (fetch.HasRows)
                    {
                        string aff = fetch.GetValue(1) as string;
                        if (aff != null)
                        {
                            aff = aff.Trim();
                            recipe.Affinities.Add(new CharacterAffinity() { Character = (string)fetch.GetValue(0), Affinity = (string)aff });
                        }
                        fetch.Read();
                    }
                }
            }

            return recipe;
        }

        public ObservableCollection<Recipe> GetRecipesOnStandby()
        {
            ObservableCollection<Recipe> res = new ObservableCollection<Recipe>();
            conn.Open();
            using (SQLiteCommand comm = new SQLiteCommand("SELECT R_Id FROM Recipes WHERE Status = :status", conn))
            {
                comm.Parameters.Add(new SQLiteParameter(":status", STATUS_TASTING));
                using (var read_id = comm.ExecuteReader())
                {

                    read_id.Read();
                    while (read_id.HasRows)
                    {
                        long id = (long)read_id.GetValue(0);
                        Recipe recipe = GetRecipeByID(id, STATUS_TASTING);
                        if (recipe != null)
                            res.Add(recipe);

                        read_id.Read();
                    }
                }
            }
            conn.Close();
            return res;
        }

        public void ArchiveRecipe(Recipe recipe, bool unarchive = false)
        {
            conn.Open();
            using (var comm = new SQLiteCommand("UPDATE Recipes SET Status = :status WHERE R_Id = :id", conn))
            {
                comm.Parameters.Add(new SQLiteParameter(":id", recipe.Id));
                if (unarchive)
                {
                    comm.Parameters.Add(new SQLiteParameter(":status", STATUS_TASTING));
                }
                else
                {
                    comm.Parameters.Add(new SQLiteParameter(":status", STATUS_ARCHIVED));
                }
                int res = comm.ExecuteNonQuery();
            }
            conn.Close();

            if (!unarchive)
            {
                UpdateAffinities(recipe);
            }
            
        }

        public void UpdateAffinities(Recipe recipe)
        {
            conn.Open();
            // Delete previous affinities; if a consumer is deleted this will get rid of old entries that are obviously no longer wanted and avoids complicated nested SQL because SQLite has no upsert.
            using (SQLiteCommand comm_remove_affs = new SQLiteCommand("DELETE FROM Consumers WHERE R_Id = :id", conn))
            {
                comm_remove_affs.Parameters.Add(new SQLiteParameter(":id", recipe.Id));
                comm_remove_affs.ExecuteNonQuery();
            }

            // Update affinities
            foreach (CharacterAffinity aff in recipe.Affinities)
            {
                // Don't insert NULL affinities, they are being ignored anyway when read from the database. If a consumer is listed but has no value returned from the database, they are added with a null affinity to a recipe being loaded anyway in case a new consumer is added, so need need to be redundant and save NULL affinities for known consumers when they can be treated like new consumers.
                if (aff.Affinity == null) continue;
                
                using (SQLiteCommand comm_insert_affs = new SQLiteCommand("INSERT INTO Consumers(R_Id,Consumer,Affinity) VALUES (:id, :cons, :aff)", conn))
                {
                    comm_insert_affs.Parameters.Add(new SQLiteParameter(":id", recipe.Id));
                    comm_insert_affs.Parameters.Add(new SQLiteParameter(":cons", aff.Character));
                    comm_insert_affs.Parameters.Add(new SQLiteParameter(":aff", aff.Affinity));
                    comm_insert_affs.ExecuteNonQuery();
                }
            }
            conn.Close();
        }

        public void DeleteRecipe(Recipe recipe)
        {
            conn.Open();
            List<object> ids = new List<object>(); 
            using (SQLiteCommand comm = new SQLiteCommand("SELECT R_Id FROM Recipes WHERE Hash=:hash", conn))
            {
                comm.Parameters.Add(new SQLiteParameter(":hash", recipe.GetHashCode()));
                    using (var read_id = comm.ExecuteReader()) {

                    

                    read_id.Read();
                    while (read_id.HasRows)
                    {
                        ids.Add(read_id.GetValue(0));
                        read_id.Read();
                    }
                }
            }
            foreach (object id in ids) {
                Recipe cmprec = new Recipe();
                using (SQLiteCommand comm_ids = new SQLiteCommand("SELECT * FROM Recipes INNER JOIN Ingredients ON Recipes.R_Id = Ingredients.R_Id WHERE Recipes.R_Id = :id", conn))
                {
                    comm_ids.Parameters.Add(new SQLiteParameter(":id", id));
                    var fetch = comm_ids.ExecuteReader();
                    bool first = true;
                    fetch.Read();
                    long cmpid = 0;
                    while (fetch.HasRows)
                    {
                        if (first)
                        {
                            cmpid = (long)fetch.GetValue(0);
                            cmprec.Name = (string)fetch.GetValue(1);
                            cmprec.Container = (string)fetch.GetValue(2);
                            first = false;
                        }
                        string ing = (string)fetch.GetValue(6);
                        cmprec.Ingredients.Add(new Ingredient() { Name = ing });
                        fetch.Read();
                    }

                    if (cmprec.Equals(recipe))
                    {
                        using (SQLiteCommand comm_del = new SQLiteCommand("DELETE FROM Recipes WHERE R_Id = :id", conn))
                        {
                            comm_del.Parameters.Add(new SQLiteParameter(":id", cmpid));
                            comm_del.ExecuteNonQuery();
                        }
                            
                        // SQLite doesn't properly cascade the delete...
                        using (SQLiteCommand comm_del = new SQLiteCommand("DELETE FROM Ingredients WHERE R_Id = :id", conn))
                        {
                            comm_del.Parameters.Add(new SQLiteParameter(":id", cmpid));
                            comm_del.ExecuteNonQuery();
                        }
                        using (SQLiteCommand comm_del = new SQLiteCommand("DELETE FROM Consumers WHERE R_Id = :id", conn))
                        {
                            comm_del.Parameters.Add(new SQLiteParameter(":id", cmpid));
                            comm_del.ExecuteNonQuery();
                        }
                    }
                }
            }
            conn.Close();
        }

        public void Dispose()
        {
            conn.Dispose();
        }
    }
}
