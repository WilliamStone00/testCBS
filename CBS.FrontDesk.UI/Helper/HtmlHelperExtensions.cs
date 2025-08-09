using CBS.FrontDesk.Data.Entity.Accounting;
using CrystalDecisions.Web;
//using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Helper
{
  public static class HtmlHelperProductConfigExtensions
        {
            public static MvcHtmlString ProductConfigurationTable(this HtmlHelper htmlHelper, ProductConfigurationViewModel model)
            {
                var html = new StringBuilder();

                // Header section
                html.AppendLine(@"
              <div class='container-fluid py-4'>
                  <div class='row mb-4'>
                      <div class='col-12'>
                          <div class='card shadow-lg border-0'>
                              <div class='card-body text-center py-4' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white;'>
                                  <h1 class='card-title mb-3'>
                                      <i class='fas fa-cogs me-3'></i>PRODUCT ACCOUNTING CONFIGURATION
                                  </h1>
                                  <p class='lead mb-0'>Complete accounting setup for all financial products</p>
                              </div>
                          </div>
                      </div>
                  </div>");

                // Table section
                html.AppendLine(@"
                  <div class='row'>
                      <div class='col-12'>
                          <div class='card shadow-lg border-0'>
                              <div class='card-header bg-primary text-white'>
                                  <h5 class='mb-0'>
                                      <i class='fas fa-table me-2'></i>Product Configuration Details
                                  </h5>
                              </div>
                              <div class='table-responsive' style='max-height: 600px; overflow-y: auto;'>
                                  <table class='table table-hover mb-0'>
                                      <thead class='table-primary sticky-top'>
                                          <tr>
                                              <th style='width: 200px;'>Product Information</th>
                                              <th style='width: 180px;'>Account ID</th>
                                              <th style='width: 250px;'>Account Name</th>
                                              <th>Description</th>
                                              <th style='width: 120px;'>Account Type</th>
                                              <th style='width: 120px;'>Operation Type</th>
                                              <th style='width: 100px;'>Commission</th>
                                          </tr>
                                      </thead>
                                      <tbody>");

                // Generate table rows
                foreach (var product in model.ProductConfigurations)
                {
                    for (int i = 0; i < product.accountingBookDetails.Count; i++)
                    {
                        var account = product.accountingBookDetails[i];
                        var isFirstAccount = i == 0;

                        html.AppendLine("<tr class='account-row'>");

                        // Product Information
                        html.AppendLine("<td>");
                        if (isFirstAccount)
                        {
                            html.AppendLine($@"
                          <div class='fw-bold text-primary'>{product.productCode}</div>
                          <small class='text-muted'>Code: {product.productCode}</small><br>
                          <small class='text-muted'>Book ID: {product.productAccountBookId}</small><br>
                          <span class='badge bg-info'>{product.productType}</span>");
                        }
                        html.AppendLine("</td>");

                        // Account ID
                        html.AppendLine($"<td><code class='bg-light p-1 rounded'>{account.mfiChartOfAccountId ?? "N/A"}</code></td>");

                        // Account Name
                        html.AppendLine($"<td><div class='fw-semibold'>{account.name}</div></td>");

                        // Description
                        html.AppendLine($"<td><small class='text-muted'>{account.description ?? "Standard account configuration"}</small></td>");

                        // Account Type
                        var badgeColor = GetAccountTypeBadgeColor(account.accountType);
                        html.AppendLine($"<td><span class='badge bg-{badgeColor}'>{account.accountType}</span></td>");

                        // Operation Type
                        var icon = GetOperationIcon(account.operationType);
                        html.AppendLine($"<td><i class='{icon} me-1'></i><small>{account.operationType}</small></td>");

                        // Commission
                        var commissionIcon = account.isCommissionAccount
                            ? "<i class='fas fa-check-circle text-success'></i>"
                            : "<i class='fas fa-minus-circle text-muted'></i>";
                        html.AppendLine($"<td class='text-center'>{commissionIcon}</td>");

                        html.AppendLine("</tr>");
                    }

                    if (product != model.ProductConfigurations.Last())
                    {
                        html.AppendLine("<tr><td colspan='7'><hr style='border: 2px solid rgba(128, 128, 128, 0.3);'></td></tr>");
                    }
                }

                html.AppendLine(@"
                                      </tbody>
                                  </table>
                              </div>
                          </div>
                      </div>
                  </div>");

                // Summary section
                html.AppendLine($@"
                  <div class='row mt-4'>
                      <div class='col-md-3'>
                          <div class='card text-center border-0 shadow'>
                              <div class='card-body'>
                                  <i class='fas fa-boxes fa-2x text-primary mb-2'></i>
                                  <h4 class='text-primary'>{model.TotalLoanProducts}</h4>
                                  <p class='text-muted'>Total Loan Product</p>
                              </div>
                          </div>
                      </div>
                      <div class='col-md-3'>
                          <div class='card text-center border-0 shadow'>
                              <div class='card-body'>
                                  <i class='fas fa-university fa-2x text-success mb-2'></i>
                                  <h4 class='text-success'>{model.TotalLoanNotConfiguredProducts}</h4>
                                  <p class='text-muted'>Account Configurations</p>
                              </div>
                          </div>
                      </div>
                      <div class='col-md-3'>
                          <div class='card text-center border-0 shadow'>
                              <div class='card-body'>
                                  <i class='fas fa-percentage fa-2x text-warning mb-2'></i>
                                  <h4 class='text-warning'>{model.TotalSavingProducts}</h4>
                                  <p class='text-muted'>Commission Accounts</p>
                              </div>
                          </div>
                      </div>
                      <div class='col-md-3'>
                          <div class='card text-center border-0 shadow'>
                              <div class='card-body'>
                                  <i class='fas fa-check-circle fa-2x text-success mb-2'></i>
                                  <h4 class='text-success'>CONFIGURED</h4>
                                  <p class='text-muted'>System Status</p>
                              </div>
                          </div>
                      </div>
                  </div>
              </div>");

                return MvcHtmlString.Create(html.ToString());
            }

            private static string GetAccountTypeBadgeColor(string accountType)
            {
                switch (accountType)
                {
                    case "Commission": return "success";
                    case "Savings": return "primary";
                    case "Interest": return "info";
                    case "Transit": return "warning";
                    case "Suspense": return "secondary";
                    case "Penalty": return "danger";
                    case "Provision": return "dark";
                    case "WriteOff": return "danger";
                    case "Principal": return "primary";
                    case "Tax": return "warning";
                    case "Fee": return "info";
                    case "Operational": return "secondary";
                    default: return "secondary";
                }
            }

            private static string GetOperationIcon(string operationType)
            {
                switch (operationType)
                {
                    case "CashIn": return "fas fa-arrow-down";
                    case "CashOut": return "fas fa-arrow-up";
                    case "Transfer": return "fas fa-exchange-alt";
                    case "MoneyTransfer": return "fas fa-money-bill-transfer";
                    case "Saving": return "fas fa-piggy-bank";
                    case "Interest": return "fas fa-percentage";
                    case "LocalTransit": return "fas fa-route";
                    case "LocalSuspense": return "fas fa-pause-circle";
                    case "LoanPenalty": return "fas fa-exclamation-triangle";
                    case "LoanProvisioning": return "fas fa-shield-alt";
                    case "LoanWriteOff": return "fas fa-times-circle";
                    case "LoanPrincipal": return "fas fa-coins";
                    case "LoanInterest": return "fas fa-chart-line";
                    case "LoanVAT": return "fas fa-receipt";
                    case "LoanTransit": return "fas fa-truck";
                    case "Liaison": return "fas fa-handshake";
                    default: return "fas fa-circle";
                }
            }
        }
}

