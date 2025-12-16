using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.MockData
{
    public class BalanceSheetMock
    {
        public List<Group> Groups { get; set; } = new List<Group>();

        public BalanceSheetMock() {

            Groups = new List<Group>
            {
                new Group
                {
                    Name = "CAPITAL",
                    Refernce = "EP01",
                    Items = new List<GroupItem>
                    {
                        
                        new GroupItem { Ref = "EP02", Heading = "Capitalized costs", Gross = 950000m, AmortProvision = 150000m, NetN = 800000m, NetN1 = 735000m },
                        new GroupItem { Ref = "EP03", Heading = "Intangible fixed assets", Gross = 675000m, AmortProvision = 225000m, NetN = 450000m, NetN1 = 400000m },
                        new GroupItem { Ref = "EP04", Heading = "Lands", Gross = 1350000m, AmortProvision = 0m, NetN = 1350000m, NetN1 = 1200000m },
                        new GroupItem { Ref = "EP05", Heading = "Other tangible assets", Gross = 820000m, AmortProvision = 220000m, NetN = 600000m, NetN1 = 570000m },
                        new GroupItem { Ref = "EP06", Heading = "Fixed assets under construction, Advanced payments", Gross = 430000m, AmortProvision = 0m, NetN = 430000m, NetN1 = 300000m },
                        new GroupItem { Ref = "EP07", Heading = "Deposits and guarantees", Gross = 150000m, AmortProvision = 0m, NetN = 150000m, NetN1 = 140000m },
                        new GroupItem { Ref = "EP08", Heading = "Equity securities and government papers", Gross = 890000m, AmortProvision = 50000m, NetN = 840000m, NetN1 = 820000m }
            
                    }
                },

                new Group
                {
                    Name = "CREDIT TO MEMBERS",
                    Refernce = "EP09",
                    Items = new List<GroupItem>
                    {
                        new GroupItem { Ref = "EP09", Heading = "Short-term credit to members", Gross = 1600000m, AmortProvision = 250000m, NetN = 1350000m, NetN1 = 1200000m },
                        new GroupItem { Ref = "EP10", Heading = "Medium-term credit to members", Gross = 2100000m, AmortProvision = 350000m, NetN = 1750000m, NetN1 = 1650000m },
                        new GroupItem { Ref = "EP11", Heading = "Long-term credit to members", Gross = 2600000m, AmortProvision = 500000m, NetN = 2100000m, NetN1 = 1980000m },
                        new GroupItem { Ref = "EP12", Heading = "Credit in arrears", Gross = 720000m, AmortProvision = 250000m, NetN = 470000m, NetN1 = 380000m },
                        new GroupItem { Ref = "EP13", Heading = "Restructured loans", Gross = 610000m, AmortProvision = 210000m, NetN = 400000m, NetN1 = 360000m },
                        new GroupItem { Ref = "EP14", Heading = "Overdue interest", Gross = 185000m, AmortProvision = 55000m, NetN = 130000m, NetN1 = 120000m },
                        new GroupItem { Ref = "EP15", Heading = "Penalty interest", Gross = 95000m, AmortProvision = 25000m, NetN = 70000m, NetN1 = 65000m },
                        new GroupItem { Ref = "EP16", Heading = "Other credit to members", Gross = 330000m, AmortProvision = 80000m, NetN = 250000m, NetN1 = 240000m }
                    }
                },

                new Group
                {
                    Name = "GOODS INVENTORY AND SIMILAR OPERATIONS",
                    Refernce = "EP17",
                    Items = new List<GroupItem>
                    {
                        new GroupItem { Ref = "EP17", Heading = "Goods inventory", Gross = 880000m, AmortProvision = 120000m, NetN = 760000m, NetN1 = 720000m },
                        new GroupItem { Ref = "EP18", Heading = "Raw materials and consumables", Gross = 450000m, AmortProvision = 60000m, NetN = 390000m, NetN1 = 360000m },
                        new GroupItem { Ref = "EP19", Heading = "Finished products", Gross = 530000m, AmortProvision = 75000m, NetN = 455000m, NetN1 = 420000m },
                        new GroupItem { Ref = "EP20", Heading = "Goods in transit", Gross = 270000m, AmortProvision = 30000m, NetN = 240000m, NetN1 = 220000m },
                        new GroupItem { Ref = "EP21", Heading = "Goods awaiting delivery", Gross = 310000m, AmortProvision = 40000m, NetN = 270000m, NetN1 = 250000m },
                        new GroupItem { Ref = "EP22", Heading = "Goods entrusted to third parties", Gross = 150000m, AmortProvision = 20000m, NetN = 130000m, NetN1 = 125000m },
                        new GroupItem { Ref = "EP23", Heading = "Discounted merchandise awaiting payment", Gross = 200000m, AmortProvision = 35000m, NetN = 165000m, NetN1 = 150000m },
                        new GroupItem { Ref = "EP24", Heading = "Other inventory operations", Gross = 175000m, AmortProvision = 25000m, NetN = 150000m, NetN1 = 140000m }
                    }
                },

            };
        }
    }

    public class Group {

        public string Name { get; set; }
        public string Refernce { get; set; }
        
      public List<GroupItem> Items { get; set; } = new List<GroupItem>();

    }

    public class GroupItem
    {
        public string Ref { get; set; }
        public string Heading { get; set; }
        public decimal? Gross { get; set; }
        public decimal? AmortProvision { get; set; }
        public decimal? NetN { get; set; }
        public decimal? NetN1 { get; set; }
    }

   
}
