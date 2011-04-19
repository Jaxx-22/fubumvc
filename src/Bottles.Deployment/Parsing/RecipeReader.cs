using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;

namespace Bottles.Deployment.Parsing
{
    public class RecipeReader
    {
        public static Recipe ReadFrom(string directory)
        {
            return new RecipeReader(directory).Read();
        }

        private readonly string _directory;
        private readonly IFileSystem _fileSystem = new FileSystem();

        public RecipeReader(string directory)
        {
            _directory = directory;
        }

        public Recipe Read()
        {
            var recipeName = Path.GetFileName(_directory);
            var recipe = new Recipe(recipeName);

            _fileSystem.ReadTextFile(FileSystem.Combine(_directory, ProfileFiles.RecipesControlFile),s =>
                {
                    //TODO: Harden this for bad syntax
                    var parts = s.Split(':');
                    recipe.RegisterDependency(parts[1]);
                });


            _fileSystem.FindFiles(_directory, new FileSet(){
                Include = "*.host"
            }).Each(file =>
            {
                var host = HostReader.ReadFrom(file);
                recipe.RegisterHost(host);
            });

            return recipe;
        }

        public static IEnumerable<Recipe> ReadRecipes(string recipesDir)
        {
            return Directory.GetDirectories(recipesDir).Select(ReadFrom);
        }
    }
}