using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Firmeo.Common.Logger;
using Firmeo.Common.PageEngine.AbstractPages;
using Firmeo.Common.General.Utils;
using Firmeo.Core.Project.Reports.Storage;
using Firmeo.Core.Project.Reports;
using Firmeo.Common.Reporting.XtraReportControls;
using Firmeo.Common.DB.Util;
using Firmeo.Common.Environment.Containers;
using Firmeo.Common.Environment.Routing;
using Firmeo.Common.DB;
using Firmeo.Core.Customer.Storages;
using Firmeo.Core.Employee.Storages;
using Firmeo.Common.General.Consts;
using Firmeo.Core.ZUK.Storages;
using Firmeo.Core.ZUK.Print.Storage;
using Firmeo.Core.ZUK.Print;
using DevExpress.XtraReports.UI;

namespace Firmeo.UI.ZUK.Reports
{
    public partial class WorkTimeEstimateReport : AbstractReportPage
    {
        private static readonly WebLogger LOGGER =
            WebLogger.GetLogger(typeof(WorkTimeEstimateReport));

        private int? _prjId = null;

        public const string PROJECT_SIMPLE_RAPORT_PARAM = ParametersConsts.PROJECT_SIMPLE_RAPORT_PARAM;

        protected bool IsSimpleRaportView()
        {
            string value = Request.QueryString[PROJECT_SIMPLE_RAPORT_PARAM];
            return !string.IsNullOrEmpty(value);
        }


        protected Dictionary<string, string> reportFilterParameter() 
        {
            Dictionary<string, string> reportParameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(tbMaterial.Text))
            {
                reportParameters.Add("materialType", tbMaterial.Text);
            }

            if (!string.IsNullOrEmpty(tbPlanNo.Text))
            {
                reportParameters.Add("planNo", tbPlanNo.Text);
            }

            if (!string.IsNullOrEmpty(tbSize.Text))
            {
                reportParameters.Add("productSize", tbSize.Text);
            }
            
                   
            return reportParameters;
        }

        protected List<WorkTimeEstimateDataSet.WorkTimeItemsRow> dataSorting(WorkTimeEstimateDataSet data)
        {
            var items = new List<WorkTimeEstimateDataSet.WorkTimeItemsRow>();

            if (tbMaterial.Text != null && tbMaterial.Text != "")
            {
                items = data.WorkTimeItems.Where(x => !x.PrName.ToLower().Contains("blacha")).ToList();
            }
            return items;
        }

       


        protected override void OnReportGenerating(object sender, ReportGeneratingEventArgs e)
        {
            try
            {
                               
                WorkTimeEstimateStorage storage = WorkTimeEstimateStorage.CreateInstance(UserUtil.getCurrentSchema());
                WorkTimeEstimateDataSet data = new WorkTimeEstimateDataSet();
                XtraReport report = new XtraReport();

                if (IsSimpleRaportView())
                {

                    sortingPanel.Visible = true;
                    searchingPanel.Visible = true;
                    rbAll.Enabled = false;
                    rbMontaz.Enabled = false;
                    rbObrobka.Enabled = false;
                    
                   
                    string materialSortOrder = "";
                    string sizeSortOrder = "";

                    if (!string.IsNullOrEmpty(dbMaterialSorting.Text))
                    {
                        materialSortOrder = dbMaterialSorting.Text;
                    }
                    if (!string.IsNullOrEmpty(dbSizeSorting.Text))
                    {
                        sizeSortOrder = dbSizeSorting.Text;
                    }


                    report = new WorkTimeEstimateXtraReportSimple(Page.Theme) { materialSortOrder = materialSortOrder, sizeSortOrder = sizeSortOrder };
                 
                    //overload for a flat DataSet
                    data = storage.GetWorkTimeEstimateToPrint(
                                                     UserUtil.getCurrentAppId(),
                                                     project.GetValueInt32().HasValue ? project.GetValueInt32().Value : 0,
                                                     departamentAll.GetValueString(), "rbAll", reportFilterParameter());
   
                }
                else 
                {
                    if (rbAll.Checked)
                    {
                        report = new WorkTimeEstimateXtraReport(Page.Theme);
                        data = storage.GetWorkTimeEstimateToPrint(
                                                         UserUtil.getCurrentAppId(),
                                                         project.GetValueInt32().HasValue ? project.GetValueInt32().Value : 0,
                                                         departamentAll.GetValueString(), "rbAll");
                    }
                    else if (rbMontaz.Checked)
                    {
                        report = new WorkTimeEstimateInstallationXtraReport(Page.Theme);
                        data = storage.GetWorkTimeEstimateToPrint(
                                                         UserUtil.getCurrentAppId(),
                                                         project.GetValueInt32().HasValue ? project.GetValueInt32().Value : 0,
                                                         departamentInstl.GetValueString(), "rbMontaz");
                    }
                    else // rbObrobka
                    {
                        report = new WorkTimeEstimateWorkingXtraReport(Page.Theme);
                        //data = storage.GetWorkTimeEstimateWorkingToPrint(
                        data = storage.GetWorkTimeEstimateToPrint(
                                                         UserUtil.getCurrentAppId(),
                                                         project.GetValueInt32().HasValue ? project.GetValueInt32().Value : 0,
                                                         departamentWork.GetValueString(), "rbObrobka");
                    }
                }             
                
                report.DataSource = data;
                e.Report = report;
            }
            catch (Exception ex)
            {
                if (DbUserErrorUtil.IsUserError(ex))
                {
                    this.SetErrorStatus(
                        DbUserErrorUtil.GetErrorMessage(ex, this.GetGlobalResourceObject("FormControl", "error").ToString()));

                    e.Cancel = true;
                }
                else
                {
                    LOGGER.Error(ex);
                    throw ex;
                }
            }
        }

    
    }
}
