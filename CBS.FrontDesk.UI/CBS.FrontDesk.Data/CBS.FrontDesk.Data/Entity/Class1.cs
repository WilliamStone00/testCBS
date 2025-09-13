using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity
{
    public abstract class BaseEntity
    {
        private DateTime _createdDate;

        public DateTime CreatedDate
        {
            get => _createdDate.ToLocalTime();
            set => _createdDate = value.ToLocalTime();
        }
        private string _bankId;

        public string BankId
        {
            get => _bankId;
            set => _bankId = value;
        }
        private string _branchId;

        public string BranchId
        {
            get => _branchId;
            set => _branchId = value;
        }
        public string CreatedBy { get; set; } = "b956f743-f846-4d83-8804-4046f259b5cf";

        public string FullName
        {
            get => _fullName;
            set => _fullName = value;
        }

        private DateTime _modifiedDate;

        public DateTime ModifiedDate
        {
            get => _modifiedDate.ToLocalTime();
            set => _modifiedDate = value.ToLocalTime();
        }

        public string ModifiedBy { get; set; } = "b956f743-f846-4d83-8804-4046f259b5cf";
        private DateTime _deletedDate;

        public DateTime DeletedDate
        {
            get => _deletedDate.ToLocalTime();
            set => _deletedDate = value.ToLocalTime();
        }

        public string DeletedBy { get; set; }

 

        public bool IsDeleted { get; set; } = false;

        private string _TempData = "";
        private string _fullName;

        public string TempData
        {
            get => _TempData;
            set => _TempData = value;
        }
        public BaseEntity GetBaseEntity()
        {
            return this;
        }

    }
}
