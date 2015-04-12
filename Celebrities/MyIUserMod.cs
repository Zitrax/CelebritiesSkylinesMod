using ICities;
using UnityEngine;

namespace Celebrities
{

    public class CelebritiesMod: IUserMod
    {

        public string Name 
        {
            get { return "Celebrities"; }
        }

        public string Description 
        {
            get { return "Adds celebrities to the game"; }
			
        }
    }

    // Inherit interfaces and implement your mod logic here
    // You can use as many files and subfolders as you wish to organise your code, as long
    // as it remains located under the Source folder.
}
