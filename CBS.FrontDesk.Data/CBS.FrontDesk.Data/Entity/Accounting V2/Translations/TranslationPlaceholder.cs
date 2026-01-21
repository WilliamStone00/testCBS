using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.Translations
{
    public class TranslationPlaceholder
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Placeholder is required")]
        [StringLength(200, ErrorMessage = "Placeholder cannot exceed 200 characters")]
        public string Placeholder { get; set; }

        [Required(ErrorMessage = "English translation is required")]
        public string English { get; set; }

        [Required(ErrorMessage = "French translation is required")]
        public string French { get; set; }

        [Required(ErrorMessage = "View is required")]
        public string View { get; set; } // Options: Global View, Main View, JAVASCRIPT View, Controller View

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

    }

    // Query model for filtering
    public class TranslationPlaceholderQuery
    {
        public DataTableOptions Options { get; set; }
        public TranslationPlaceholderQuery() { Options = new DataTableOptions(); }

        public string Placeholder { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string View { get; set; }

    }

    // Add this model class
    public class ExportTranslationPlaceholderRequest
    {
        public List<TranslationPlaceholder> TableData { get; set; }
        public TranslationPlaceholderQuery Query { get; set; }
        public string ExportedBy { get; set; }
        public DateTime ExportDate { get; set; }
    }
}

