using CBS.FrontDesk.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Helper
{
    public class MenuLoaderHelper
    {
        public List<PermissionMenuLoader> AddParentNames(List<PermissionMenuLoader> menuLoaders)
        {
            // Create a dictionary to store parent names by their IDs
            Dictionary<int, string> parentNames = new Dictionary<int, string>();

            // Populate the dictionary with parent names
            foreach (var menuLoader in menuLoaders)
            {
                if (!parentNames.ContainsKey(menuLoader.ParentId))
                {
                    parentNames[menuLoader.ParentId] = "Root-Parent";
                }

                // If the ParentId is not 0, find the parent name
                if (menuLoader.ParentId != 0)
                {
                    parentNames[menuLoader.MenuMasterId] = menuLoaders.FirstOrDefault(m => m.MenuMasterId == menuLoader.ParentId)?.MenuText ?? "";
                }
            }

            // Add the parent names to the menu loaders
            foreach (var menuLoader in menuLoaders)
            {
                menuLoader.ParentName = parentNames[menuLoader.ParentId];
            }

            return menuLoaders;
        }
    }
}
