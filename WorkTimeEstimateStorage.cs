using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firmeo.Common.DB.Storage;
using Firmeo.Common.Logger;
using Firmeo.Common.DB.ConnectionProvider;
using Firmeo.Common.DB;
using Oracle.DataAccess.Client;
using System.Data;
using Firmeo.Common.DB.Util;
using Firmeo.Core.ZUK.Print.Storage;
using Firmeo.Common.DB.Readers;
using Oracle.DataAccess.Types;
using Firmeo.Core.ZUK.Containers;
using Firmeo.Common.General.Utils;
using System.Reflection;

namespace Firmeo.Core.ZUK.Storages
{
    public class WorkTimeEstimateStorage : AbstractStorage
    {
      

       
        public WorkTimeEstimateDataSet GetWorkTimeEstimateToPrint(
          int appId,
          int prjId,
          string depCode,
          string reportType,
          Dictionary<string, string> param)
        {
            LOGGER.Finest("Entering GetWorkTimeEstimateToPrint");

            OracleConnection connection = null;

            OracleParameter appIdParam = null;
            OracleParameter prjIdParam = null;
            OracleParameter depCodeParam = null;
            OracleParameter reportTypeParam = null;
            OracleParameter prjNumberParam = null;
            OracleParameter prjNameParam = null;
            OracleParameter weightParam = null;
            OracleParameter qtyParam = null;
            OracleParameter repDateParam = null;
            OracleParameter authorParam = null;
            OracleParameter itemsParam = null;
            OracleParameter itemsDepParam = null;
            NullableDataReader nullableReader = null;


            //
            //
            WorkTimeEstimateDataSet result = new WorkTimeEstimateDataSet();
            //
            //



            OracleCommand cmd = new OracleCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            try
            {
                connection = getConnection();
                cmd.CommandText = "WB_PCK_REPORTS_WORK.get_work_time_estimate";
                cmd.Connection = connection;

                appIdParam = SQLUtil.CreateInt32OracleParam("P_APP_ID", cmd, appId, ParameterDirection.Input);
                prjIdParam = SQLUtil.CreateInt32OracleParam("p_prj_id", cmd, prjId, ParameterDirection.Input);
                depCodeParam = SQLUtil.CreateVarcharOracleParam("p_dep_code", cmd, depCode, ParameterDirection.Input);
                reportTypeParam = SQLUtil.CreateVarcharOracleParam("p_report_type", cmd, reportType, ParameterDirection.Input);
                prjNumberParam = SQLUtil.CreateVarcharOracleParam("o_prj_number", cmd, null, ParameterDirection.Output);
                prjNameParam = SQLUtil.CreateVarcharOracleParam("o_prj_name", cmd, null, ParameterDirection.Output);
                weightParam = SQLUtil.CreateInt32OracleParam("o_weight", cmd, null, ParameterDirection.Output);
                qtyParam = SQLUtil.CreateInt32OracleParam("o_qty", cmd, null, ParameterDirection.Output);
                repDateParam = SQLUtil.CreateDateOracleParam("o_rep_date", cmd, null, ParameterDirection.Output);
                authorParam = SQLUtil.CreateVarcharOracleParam("o_author", cmd, null, ParameterDirection.Output);
                itemsParam = SQLUtil.CreateCursorOracleParameter("o_items", OracleDbType.RefCursor, cmd);
                itemsDepParam = SQLUtil.CreateCursorOracleParameter("o_items_dep", OracleDbType.RefCursor, cmd);

                cmd.ExecuteNonQuery();

                WorkTimeEstimateDataSet.WorkTimeMainTableRow row = result.WorkTimeMainTable.NewWorkTimeMainTableRow();

                row.ID = prjId.ToString();
                row.ProjNumber = SQLUtil.GetStringValue(prjNumberParam);
                row.ProjName = SQLUtil.GetStringValue(prjNameParam);
                row.Weight = SQLUtil.GetDecimalValue(weightParam).HasValue ? SQLUtil.GetDecimalValue(weightParam).ToString() : "---";
                row.Qty = SQLUtil.GetDecimalValue(qtyParam).HasValue ? SQLUtil.GetDecimalValue(qtyParam).ToString() : "---";
                row.AllWeight = SQLUtil.GetDecimalValue(weightParam).HasValue ? SQLUtil.GetDecimalValue(weightParam).ToString() : "---";
                row.ReportDate = SQLUtil.GetDateValue(repDateParam).HasValue ? SQLUtil.GetDateValue(repDateParam).Value.Date.ToShortDateString() : "---";
                row.Author = SQLUtil.GetStringValue(authorParam);

                result.WorkTimeMainTable.AddWorkTimeMainTableRow(row);

                if (!((OracleRefCursor)itemsParam.Value).IsNull)
                {
                    nullableReader = new NullableDataReader(((OracleRefCursor)itemsParam.Value).GetDataReader());

                    string TYPE = string.Empty;
                    string name = string.Empty;
                    string drawing_number = string.Empty;

                    string activePath = string.Empty;
                    WorkTimeEstimateDataSet.WorkTimePathRow path = null;


                    while (nullableReader.Read())
                    {
                        string iPath = ConvertUtil.ConvertToSpecialChars(nullableReader.GetNullableString("pwei_path"));

                        if (string.IsNullOrEmpty(iPath))
                            continue;

                        WorkTimeEstimateDataSet.WorkTimeItemSimpleRow item = result.WorkTimeItemSimple.NewWorkTimeItemSimpleRow();


                        item.ID = prjId.ToString();
                        item.ItemID = nullableReader.GetNullableDecimal("pwei_id").ToString();
                        item.IsProduct = nullableReader.GetNullableDecimal("pwei_is_product").ToString();
                        item.ParentID = nullableReader.GetNullableDecimal("pwei_parent_pwei_id").ToString();

                        TYPE = nullableReader.GetNullableString("pwei_itm_type");
                        name = ConvertUtil.ConvertToSpecialChars(nullableReader.GetNullableString("pwei_name"));

                        drawing_number = ConvertUtil.ConvertToSpecialChars(nullableReader.GetNullableString("pwei_drawing_number"));



                        item.DrawingNumber = drawing_number;
                        item.Qty = nullableReader.GetNullableDecimal("pwei_qty").ToString();
                        item.QtyPerProj = nullableReader.GetNullableDecimal("pwei_qty_per_prj").ToString();
                        if (nullableReader.GetNullableString("pwei_product_brand") != null)
                        {
                            item.ProductBrand = nullableReader.GetNullableString("pwei_product_brand");
                        }
                        else
                        {
                            item.ProductBrand = "";
                        }

                        string tmp_sizeCut = nullableReader.GetNullableString("pwei_size_cut");
                        string tmp_addInfo = nullableReader.GetNullableString("pwei_add_info");
                        item.SizeCut = string.IsNullOrEmpty(tmp_sizeCut) ? "" : ConvertUtil.ConvertToSpecialChars(tmp_sizeCut.Replace("X", " x ").Replace("L=", "L= "));
                        if (!string.IsNullOrEmpty(tmp_addInfo))
                            item.SizeCut += string.Format("\n{0}", tmp_addInfo);
                        item.SizeCut = ConvertUtil.ConvertToSpecialChars(item.SizeCut);

                        if (param.ContainsKey("productSize"))
                        {
                            if (!item.SizeCut.ToLower().Contains(param["productSize"]))
                            {
                                continue;
                            }
                        }

                        // material gatunek
                        string tempPrName = ConvertUtil.ConvertToSpecialChars(nullableReader.GetNullableString("pwei_pr_name"));
                        if (!string.IsNullOrEmpty(tempPrName))
                        {
                            item.PrName = tempPrName + "\n" + ConvertUtil.ConvertToSpecialChars(item.ProductBrand);
                        }
                        else
                        {
                            item.PrName = " " + "\n" + ConvertUtil.ConvertToSpecialChars(item.ProductBrand);
                        }

                        if (param.ContainsKey("materialType"))
                        {
                            if (!item.PrName.ToLower().Contains(param["materialType"]))
                            {
                                continue;
                            }
                        }

                        item.ProgramNumber = ConvertUtil.ConvertToSpecialChars(nullableReader.GetNullableString("pwei_program_number"));

                        decimal? value = nullableReader.GetNullableDecimal("pwei_ls");
                        item.LS = value.HasValue ? string.Format("{0:f2}", value.Value) : "";
                        if (item.LS.Equals("0,000")) item.LS = "0";
                        value = nullableReader.GetNullableDecimal("pwei_es");
                        item.ES = value.HasValue ? string.Format("{0:f2}", value.Value) : "";
                        if (item.ES.Equals("0,000")) item.ES = "0";
                        value = nullableReader.GetNullableDecimal("pwei_bn");
                        item.BN = value.HasValue ? string.Format("{0:f2}", value.Value) : "";
                        if (item.BN.Equals("0,000")) item.BN = "0";
                        value = nullableReader.GetNullableDecimal("pwei_pt");
                        item.PT = value.HasValue ? string.Format("{0:f2}", value.Value) : "";
                        if (item.PT.Equals("0,000")) item.PT = "0";
                        value = nullableReader.GetNullableDecimal("pwei_bw");
                        item.BW = value.HasValue ? string.Format("{0:f2}", value.Value) : "";
                        if (item.BW.Equals("0,000")) item.BW = "0";
                        value = nullableReader.GetNullableDecimal("pwei_bz");
                        item.BZ = value.HasValue ? string.Format("{0:f2}", value.Value) : "";
                        if (item.BZ.Equals("0,000")) item.BZ = "0";
                        value = nullableReader.GetNullableDecimal("pwei_ph");
                        item.PH = value.HasValue ? string.Format("{0:f2}", value.Value) : "";
                        if (item.PH.Equals("0,000")) item.PH = "0";
                        value = nullableReader.GetNullableDecimal("pwei_wp");
                        item.WP = value.HasValue ? string.Format("{0:f2}", value.Value) : "";
                        if (item.WP.Equals("0,000")) item.WP = "0";
                        value = nullableReader.GetNullableDecimal("pwei_rp");
                        item.RP = value.HasValue ? string.Format("{0:f2}", value.Value) : "";
                        if (item.RP.Equals("0,000")) item.RP = "0";
                        value = nullableReader.GetNullableDecimal("pwei_po");
                        item.PO = value.HasValue ? string.Format("{0:f2}", value.Value) : "";
                        if (item.PO.Equals("0,000")) item.PO = "0";
                        value = nullableReader.GetNullableDecimal("pwei_mw");
                        item.MW = value.HasValue ? string.Format("{0:f2}", value.Value) : "";
                        if (item.MW.Equals("0,000")) item.MW = "0";
                        value = nullableReader.GetNullableDecimal("pwei_mc");
                        item.MC = value.HasValue ? string.Format("{0:f2}", value.Value) : "";
                        if (item.MC.Equals("0,000")) item.MC = "0";
                        value = nullableReader.GetNullableDecimal("pwei_ML");
                        item.ML = value.HasValue ? string.Format("{0:f2}", value.Value) : "";
                        if (item.ML.Equals("0,000")) item.ML = "0";
                        value = nullableReader.GetNullableDecimal("pwei_KO");
                        item.KO = value.HasValue ? string.Format("{0:f2}", value.Value) : "";
                        if (item.KO.Equals("0,000")) item.KO = "0";

                        item.PlanNumber = nullableReader.GetNullableDecimal("pwei_lp").HasValue ?
                            nullableReader.GetNullableDecimal("pwei_lp").Value.ToString() : "";

                        if (param.ContainsKey("planNo"))
                        {

                            if (!(item.PlanNumber.ToLower() == param["planNo"].ToLower()))
                            {
                                continue;
                            }
                        }

                        item.WorkTimeMainTableRow = row;

                        result.WorkTimeItemSimple.AddWorkTimeItemSimpleRow(item);

                    }
                }
            }

            catch (Exception ex)
            {
                LOGGER.Error("Problem occured while GetWorkTimeEstimateToPrint!" + ex.Message, ex);
                throw new Exception("Problem occured while GetWorkTimeEstimateToPrint!" + ex.Message, ex);
            }
            finally
            {
                SQLUtil.Close(appIdParam);
                SQLUtil.Close(prjIdParam);
                SQLUtil.Close(depCodeParam);
                SQLUtil.Close(reportTypeParam);
                SQLUtil.Close(prjNumberParam);
                SQLUtil.Close(prjNameParam);
                SQLUtil.Close(weightParam);
                SQLUtil.Close(qtyParam);
                SQLUtil.Close(repDateParam);
                SQLUtil.Close(authorParam);
                SQLUtil.Close(itemsParam);
                SQLUtil.Close(itemsDepParam);
                SQLUtil.Close(nullableReader);
                SQLUtil.Close(cmd);
                ReleaseConnection(connection);
            }


            return result;
        }

    }    
}

