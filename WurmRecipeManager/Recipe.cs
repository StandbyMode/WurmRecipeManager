using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace WurmRecipeManager
{
    internal class IngredientComparer : IEqualityComparer<Ingredient>
    {

        public bool Equals(Ingredient x, Ingredient y)
        {
            return x.Name.Equals(y.Name);
        }

        public int GetHashCode(Ingredient obj)
        {
            return obj.Name.GetHashCode();
        }
    }

    // Small string container for UI bindings
    public class Ingredient
    {
        public String Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

        // Messes with removing things from the ObservableCollection. Use the appropiate comparator above instead when needed.
        //public override bool Equals(object obj)
        //{
        //    Ingredient that = obj as Ingredient;
        //    if (that != null && this.Name != null)
        //    {
        //        return this.Name.Equals(that.Name);
        //    }
        //    return false;
        //}
    }

    public class CharacterAffinity {
        public String Character { get; set; }
        public String Affinity { get; set; }

        public override string ToString()
        {
            return Character + ": " + Affinity;
        }
    }

    public class Recipe : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void PropChange(String property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        // Serialiser assigned ID
        private long _id;
        public long Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                PropChange("ID");
            }
        }

        private String _name;
        public String Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                PropChange("Name");
            }
        }

        private String _container;
        public String Container
        {
            get
            {
                return _container;
            }
            set
            {
                _container = value;
                PropChange("Container");
            }
        }

        private ObservableCollection<Ingredient> _ingredients;
        public ObservableCollection<Ingredient> Ingredients
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

        private List<CharacterAffinity> _affinities;
        public List<CharacterAffinity> Affinities
        {
            get
            {
                return _affinities;
            }
            set
            {
                _affinities = value;
                PropChange("Affinities");
            }
        }

        public String TooltipString
        {
            get
            {
                List<String> ings = Ingredients.Select(i => i.Name).ToList();
                ings.Sort();
                return "Cooked in:\n" + Container + "\nIngredients:\n" + ings.Aggregate((s1, s2) => s1 + ", " + s2);
            }
        }

        public Recipe(List<String> consumers = null)
        {
            Ingredients = new ObservableCollection<Ingredient>();
            if (consumers != null)
                Affinities = consumers.Select(s => new CharacterAffinity() { Character = s }).ToList();
            else
                Affinities = new List<CharacterAffinity>();
        }

        public override int GetHashCode()
        {
            List<String> ings =  Ingredients.Select(i => i.Name).ToList();
            ings.Sort();
            return (Container + ings.Aggregate((s1, s2) => s1 + s2)).GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            Recipe that = obj as Recipe;
            if (that != null)
            {
                var l1 = this.Ingredients.ToList();
                var l2 = that.Ingredients.ToList();

                l1.Sort((i1,i2) => (i1.Name.CompareTo(i2.Name)));
                l2.Sort((i1,i2) => (i1.Name.CompareTo(i2.Name)));

                return this.Container.Equals(that.Container) && l1.SequenceEqual(l2, new IngredientComparer());
            }
            return false;
        }
    }
}
