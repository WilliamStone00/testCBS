using CBS.FrontDesk.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.MenuTranslationP
{
    public class MenuTranslation
    {
        public string Id { get; set; }
        public int MenuMasterId { get; set; }
        public string LanguageCode { get; set; }
        public string TranslatedText { get; set; }
        public string Tooltip { get; set; }
        public string Description { get; set; }
        public MenuMaster MenuMaster { get; set; }
        public List<MenuMaster> MenuMasters { get; set; }
    }
    public class GetUntranslatedMenuMastersQuery
    {
        public string LanguageCode { get; set; } = "en";
    }
}
public class MenuTranslation
{
    public string Id { get; set; }
    public int MenuMasterId { get; set; }
    public string LanguageCode { get; set; }
    public string TranslatedText { get; set; }
    public string Tooltip { get; set; }
    public string Description { get; set; }
    public string MenuText { get; set; }
    public string OriginalTooltip { get; set; }
    public string OriginalDescription { get; set; }
    public MenuMaster MenuMaster { get; set; }
}
public class AddMenuTranslationCommand
{
    public List<MenuTranslation> Translations { get; set; }
}