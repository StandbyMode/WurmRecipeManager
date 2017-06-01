using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections.ObjectModel;


namespace WurmRecipeManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void PropChange(String property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        private List<String> _ingredients;
        public List<String> Ingredients
        {
            get
            {
                return _ingredients;
            }
            set
            {
                _ingredients = value;
                PropChange("Ingredients");
            }
        }

        private Recipe _currentRecipe;
        public Recipe CurrentRecipe
        {
            get
            {
                return _currentRecipe;
            }
            set
            {
                _currentRecipe = value;
                PropChange("CurrentRecipe");
            }
        }

        private ObservableCollection<Recipe> _recipesToTaste;
        public ObservableCollection<Recipe> RecipesToTaste
        {
            get
            {
                return _recipesToTaste;
            }
            set
            {
                _recipesToTaste = value;
                PropChange("RecipesToTaste");
            }
        }

        private List<String> _consumers;
        public List<String> Consumers
        {
            get
            {
                return _consumers;
            }
            set
            {
                _consumers = value;
                PropChange("Consumers");
            }
        }

        private List<String> _containers;
        public List<String> Containers
        {
            get
            {
                return _containers;
            }
            set
            {
                _containers = value;
                PropChange("Containers");
            }
        }

        private List<String> _skills;
        public List<String> Skills
        {
            //get
            //{
            //    return new List<String>() {
            //        "Unassigned",
            //        "Body",
            //        "Body control",
            //        "Body stamina",
            //        "Body strength",
            //        "Mind",
            //        "Mind logic",
            //        "Mind speed",
            //        "Soul",
            //        "Soul depth",
            //        "Soul strength",
            //        "Alchemy",
            //        "Natural substances",
            //        "Archery",
            //        "Short bow",
            //        "Medium bow",
            //        "Long bow",
            //        "Axes",
            //        "Small axe",
            //        "Hatchet",
            //        "Large axe",
            //        "Huge axe",
            //        "Carpentry",
            //        "Bowyery",
            //        "Fletching",
            //        "Fine carpentry",
            //        "Toy making",
            //        "Ship building",
            //        "Climbing ",
            //        "Clubs",
            //        "Huge club",
            //        "Coal-making ",
            //        "Cooking",
            //        "Hot food cooking",
            //        "Baking",
            //        "Dairy food making",
            //        "Butchering",
            //        "Beverages",
            //        "Digging ",
            //        "Fighting",
            //        "Weaponless fighting",
            //        "Aggressive fighting",
            //        "Normal fighting",
            //        "Defensive fighting",
            //        "Taunting",
            //        "Shield bashing",
            //        "Firemaking ",
            //        "Hammers",
            //        "Warhammer",
            //        "Healing",
            //        "First aid",
            //        "Knives",
            //        "Carving knife",
            //        "Butchering knife",
            //        "Masonry",
            //        "Stone cutting",
            //        "Mauls",
            //        "Small maul",
            //        "Medium maul",
            //        "Large maul",
            //        "Milling ",
            //        "Mining ",
            //        "Miscellaneous items",
            //        "Shovel",
            //        "Rake",
            //        "Hammer",
            //        "Saw",
            //        "Sickle",
            //        "Scythe",
            //        "Repairing",
            //        "Pickaxe",
            //        "Stone chisel",
            //        "Nature",
            //        "Fishing",
            //        "Farming",
            //        "Forestry",
            //        "Milking",
            //        "Foraging",
            //        "Botanizing",
            //        "Gardening",
            //        "Animal taming",
            //        "Animal husbandry",
            //        "Meditating",
            //        "Papyrusmaking",
            //        "Paving ",
            //        "Polearms",
            //        "Halberd",
            //        "Long spear",
            //        "Staff",
            //        "Pottery ",
            //        "Prospecting ",
            //        "Religion",
            //        "Preaching",
            //        "Prayer",
            //        "Channeling",
            //        "Exorcism",
            //        "Ropemaking ",
            //        "Shields",
            //        "Small wooden shield",
            //        "Medium wooden shield",
            //        "Large wooden shield",
            //        "Medium shield",
            //        "Small metal shield",
            //        "Large metal shield",
            //        "Smithing",
            //        "Weapon smithing",
            //        "Blades smithing",
            //        "Weapon heads smithing",
            //        "Armour smithing",
            //        "Chain armour smithing",
            //        "Plate armour smithing",
            //        "Shield smithing",
            //        "Blacksmithing",
            //        "Locksmithing",
            //        "Metallurgy",
            //        "Jewelry smithing",
            //        "Swords",
            //        "Short sword",
            //        "Long sword",
            //        "Two handed sword",
            //        "Tailoring",
            //        "Cloth tailoring",
            //        "Leatherworking",
            //        "Thatching",
            //        "Thievery",
            //        "Stealing",
            //        "Lockpicking",
            //        "Traps",
            //        "Toys",
            //        "Yoyo",
            //        "Puppeteering",
            //        "Tracking ",
            //        "War machines",
            //        "Ballistae",
            //        "Catapults",
            //        "Trebuchets",
            //        "Turrets",
            //        "Woodcutting ",
            //    };
            get {
                return _skills;
            }
            set
            {
                _skills = value;
                PropChange("Skills");
            }
        }

        private void LoadIngredients(String filename = "ingredients.txt")
        {
            try
            {
                using (var reader = new StreamReader(File.OpenRead(filename))) {
                    String content = reader.ReadToEnd();
                    List<String> ingredients = content.Split('\n').Select(s => s.Trim()).ToList();
                    ingredients.Sort();
                    Ingredients = ingredients;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading ingredient list");
            }
        }

        private void LoadSkills(String filename = "skills.txt")
        {
            try
            {
                using (var reader = new StreamReader(File.OpenRead(filename)))
                {
                    String content = reader.ReadToEnd();
                    List<String> skills = content.Split('\n').Select(s => s.Trim()).ToList();
                    //skills.Sort();
                    Skills = skills;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading skill list");
            }
        }

        private void LoadConsumers(String filename = "consumers.txt")
        {
            try
            {
                using (var reader = new StreamReader(File.OpenRead(filename)))
                {
                    String content = reader.ReadToEnd();
                    List<String> consumers = content.Split('\n').Select(s => s.Trim()).ToList();

                    if (consumers.Count == 0 || string.IsNullOrEmpty(consumers.FirstOrDefault()))
                    {
                        MessageBox.Show("Consumer list appears to be empty. Please select\"Lists\"->\"Open consumers file\" and add one line for each of your characters.\nThen reload the list using \"Reload consumers file\"", "No consumers registered.");
                        Consumers = new List<String>();
                        return;
                    }

                    consumers.Sort();
                    Consumers = consumers;



                    foreach (Recipe recipe in RecipesToTaste.Concat(new List<Recipe>() { CurrentRecipe }))
                    {
                        if (recipe == null) continue; // Happens at startup only. In any other case this means CurrentRecipe will be updated aswell after the user edits the consuemrs file
                        foreach (string consumer in Consumers)
                        {
                            if (!recipe.Affinities.Any(ca => ca.Character.Equals(consumer)))
                            {
                                recipe.Affinities.Add(new CharacterAffinity() { Character = consumer });
                            }
                        }
                        recipe.Affinities.Sort((a1, a2) => a1.Character.CompareTo(a2.Character));
                        recipe.PropChange("Affinities"); // Fuck it, not making this another observable collection because it needs to be able to be sorted.
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading consumer list");
            }
        }

        private void LoadContainers(String filename = "containers.txt")
        {
            try
            {
                using (var reader = new StreamReader(File.OpenRead(filename)))
                {
                    String content = reader.ReadToEnd();
                    List<String> conts = content.Split('\n').Select(s => s.Trim()).ToList();
                    conts.Sort();
                    Containers = conts;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading containers list");
            }
        }

        # region commands
        public ICommand AddIngredientCommand
        {
            get;
            internal set;
        }

        private bool CanExecuteAddIngredientCommand()
        {
            return true;
        }

        private void CreateCommands()
        {
            AddIngredientCommand = new AddIngredientCommand(this);
        }

        # endregion

        private RecipeSerialiser db_manager;

        public MainWindow()
        {
            db_manager = new SQLiteSerialiser("recipes.sqlite");
            CreateCommands();

            LoadSkills();

            RecipesToTaste = db_manager.GetRecipesOnStandby();

            LoadIngredients();
            LoadConsumers();
            LoadContainers();
            
            InitializeComponent();

            // Recipes on standby might have some mismatch with a now updated consumer list. Add missing consumers and delete those who aren't in the list anymore.
            foreach (Recipe rec in RecipesToTaste)
            {
                foreach (string consumer in Consumers)
                {
                    if (!rec.Affinities.Any(ca => ca.Character.Equals(consumer)))
                    {
                        rec.Affinities.Add(new CharacterAffinity() { Character = consumer });
                    }
                }

                // Remove all affinities for which no consumer can be found in the list.
                rec.Affinities.RemoveAll(ca => !Consumers.Any(c => c.Equals(ca.Character)));

                rec.Affinities.Sort((a1, a2) => a1.Character.CompareTo(a2.Character));
            }

            CurrentRecipe = new Recipe(Consumers);
            
        }

        public void AddWorkbenchIngredient(object sender, RoutedEventArgs e)
        {
            CurrentRecipe.Ingredients.Add(new Ingredient());
        }

        private void RemoveWorkbenchIngredient(object sender, RoutedEventArgs e)
        {
            CurrentRecipe.Ingredients.Remove(((sender as Button).DataContext as Ingredient));
        }

        private void SaveRecipe(object sender, RoutedEventArgs e)
        {
            if (ContainerSelect.SelectedValue == null)
            {
                MessageBox.Show("No container selected!");
                return;
            }

            if (CurrentRecipe.Ingredients.Count == 0)
            {
                MessageBox.Show("Recipe needs at least one ingredient.");
                return;
            }

            if (CurrentRecipe.Ingredients.Any(i => string.IsNullOrEmpty(i.Name)))
            {
                MessageBox.Show("Select an ingredient for each slot or delete the slot using the '-' Button");
                return;
            }

            if (RecipesToTaste.Contains(CurrentRecipe))
            {
                MessageBox.Show("Recipe is currently being tasted. Finish it in the 'Consumers' tab.");
                return;
            }

            if (db_manager.ContainsRecipe(CurrentRecipe))
            {
                MessageBox.Show("Recipe is already known!");
                return;
            }

            var uniques = CurrentRecipe.Ingredients.Distinct(new IngredientComparer());
            if (uniques.Count() < CurrentRecipe.Ingredients.Count)
            {
                MessageBox.Show("Cannot use the same ingredient multiple times!");
                return;
            }
 
            CurrentRecipe.Name = txtRecipeName.Text;

            if (string.IsNullOrWhiteSpace(CurrentRecipe.Name))
                CurrentRecipe.Name = "Generic Food";

            txtRecipeName.Text = "";
            RecipesToTaste.Add(CurrentRecipe);
            db_manager.SaveRecipe(CurrentRecipe);
            if (sender == btnForkRecipe)
                CurrentRecipe = CurrentRecipe.Fork(); // Fork the current recipe so modifying the instance passed into the Consumers tab doesn't mess with the workbench content
            else
                CurrentRecipe = new Recipe(Consumers);
            CurrentRecipe.Container = (string)ContainerSelect.SelectedValue; // Prevent NULL containers on new recipe despite the combo box showing something fro mthe previous recipe

        }

        private void ArchiveRecipe(object sender, RoutedEventArgs e)
        {
            Recipe archived = (sender as Button).DataContext as Recipe;

            if (archived.Affinities.Any(i => string.IsNullOrEmpty(i.Affinity)))
            {
                MessageBox.Show("Not all affinities are assigned!");
                return;
            }

            RecipesToTaste.Remove(archived);
            db_manager.ArchiveRecipe(archived);
        }

        private void EditRecipe(object sender, RoutedEventArgs e)
        {
            // Move recipe back to workbench
            Recipe recipe = (sender as FrameworkElement).DataContext as Recipe;
            db_manager.DeleteRecipe(recipe);
            CurrentRecipe = recipe;
            RecipesToTaste.Remove(recipe);
        }


        private void EditRecipeName(object sender, MouseButtonEventArgs e)
        {
            (sender as EditableLabel).Editing = true;
        }

        private void EditRecipeNameLostFocus(object sender, RoutedEventArgs e)
        {
            (sender as EditableLabel).Editing = false;
            Recipe recipe = ((sender as EditableLabel).DataContext as Recipe);
            recipe.Name = (sender as EditableLabel).Text as string;
            db_manager.UpdateRecipeName(recipe);
        }

        private void EditRecipeNameKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                (sender as EditableLabel).Editing = false;
                Recipe recipe = ((sender as EditableLabel).DataContext as Recipe);
                recipe.Name = (sender as EditableLabel).txtBox.Text as string; // Go to the textbox directly because the binding doesn't update the property in time...the binding is still neccessary to update the label text, though.
                db_manager.UpdateRecipeName(recipe);
            }
        }

        private void SearchAffinity(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Recipe> recipes = db_manager.SearchAffinity(ConsumerSearch.Text, AffinitySearch.Text);

            SearchResults.DataContext = recipes;
        }

        // Replaced by binding
        //private void ContainerSelected(object sender, RoutedEventArgs e)
        //{
        //    CurrentRecipe.Container = (string)ContainerSelect.SelectedValue;
        //}

        private void ViewSearchResult(object sender, SelectionChangedEventArgs e)
        {
            //SearchResultContainer.DataContext = SearchResults.SelectedValue;
            //SearchResultIngredients.DataContext = SearchResults.SelectedValue;

            //SearchResultAffinities.DataContext = SearchResults.SelectedValue;
        }

        private void RefreshList(object sender, RoutedEventArgs e)
        {
            if (sender == RefreshConsumers)
            {
                LoadConsumers();
            }
            else if (sender == RefreshIngredients)
            {
                LoadIngredients();
            }
            else if (sender == RefreshContainers)
            {
                LoadContainers();
            }
            else if (sender == RefreshSkills)
            {
                LoadSkills();
            }
        }

        private void QuitApplication(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OpenListFile(object sender, RoutedEventArgs e)
        {
            if (sender == OpenConsumers)
            {
                System.Diagnostics.Process.Start("consumers.txt");
            }
            else if (sender == OpenIngredients)
            {
                System.Diagnostics.Process.Start("ingredients.txt");
            }
            else if (sender == OpenContainers)
            {
                System.Diagnostics.Process.Start("containers.txt");
            }
            else if (sender == OpenSkills)
            {
                System.Diagnostics.Process.Start("skills.txt");
            }
        }

        private void DeleteRecipe(object sender, RoutedEventArgs e)
        {
            Recipe recipe = SearchResults.SelectedValue as Recipe;
            if (recipe != null)
            {
                db_manager.DeleteRecipe(recipe);
                (SearchResults.DataContext as ObservableCollection<Recipe>).Remove(recipe);
            }
        }

        private void RetractRecipe(object sender, RoutedEventArgs e)
        {
            Recipe recipe = SearchResults.SelectedValue as Recipe;
            if (recipe != null)
            {
                (SearchResults.DataContext as ObservableCollection<Recipe>).Remove(recipe);

                if (sender == btnAdaptRecipe)
                {
                    
                    CurrentRecipe = recipe.Fork();
                    TabController.SelectedIndex = 0;
                    //RecipesToTaste.Remove(recipe);
                }
                else
                {
                    RecipesToTaste.Add(recipe);
                    db_manager.ArchiveRecipe(recipe, true);
                    

                    foreach (string consumer in Consumers)
                    {
                        if (!recipe.Affinities.Any(ca => ca.Character.Equals(consumer)))
                        {
                            recipe.Affinities.Add(new CharacterAffinity() { Character = consumer });
                        }
                    }
                    recipe.Affinities.Sort((a1, a2) => a1.Character.CompareTo(a2.Character));
                }
            }
        }

        private void OnClose(object sender, CancelEventArgs e)
        {
            foreach (Recipe recipe in RecipesToTaste)
            {
                // Recipes being loaded have new consumers filled in, so there's no need to fill in NULL values.
                // Write to DB
                db_manager.UpdateAffinities(recipe);
            }
            db_manager.Dispose();
        }

        private void SearchByName(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Recipe> recipes = db_manager.SearchName(txtNameSearch.Text);

            SearchResults.DataContext = recipes;
        }


        private void scrUpdateHiddenScrolls(object sender, ScrollChangedEventArgs e)
        {
            scrRecipeNames.ScrollToVerticalOffset(scrViewAffinities.VerticalOffset);
            scrRecipeNames.UpdateLayout();
            scrConsumers.ScrollToHorizontalOffset(scrViewAffinities.HorizontalOffset);
            scrConsumers.UpdateLayout();
        }






    }


}
