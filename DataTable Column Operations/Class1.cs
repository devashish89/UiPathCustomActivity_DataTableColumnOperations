using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Validation;
using System.ComponentModel;
using System.Data;

namespace DataTable_Column_Operations
{
    public class RemoveMultipleColumns:CodeActivity
    {

        [Category("Input")]
        [DisplayName("Column Indexes")]
        [Description("Array of column indexes")]
        public InArgument<int[]> ColumnIndexes { get; set; }

        [Category("Input")]
        [DisplayName("Column Names")]
        [Description("Array of column names")]
        public InArgument<string[]> ColumnNames { get; set; }


        [Category("Input")]
        [RequiredArgument]
        [DisplayName("Input / Output DataTable")]
        [Description("Input/Output DataTable")]
        public InOutArgument<System.Data.DataTable> InOutDataTable { get; set; }


        [Category("Output")]
        [RequiredArgument]
        [DisplayName("Status")]
        [Description("Status <bool>")]
        public OutArgument<bool> Status { get; set; }

        [Category("Output")]
        [DisplayName("Error Message")]
        [Description("Error Message / Empty string in case of success")]
        public OutArgument<string> ErrorMsg { get; set; }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            

            if ((ColumnIndexes != null) && (ColumnNames != null) )
            {
                ValidationError error = new ValidationError("You can NOT give both fields: Column Indexes and Column Names values");
                metadata.AddValidationError(error);
            }

            if ((ColumnIndexes == null) && (ColumnNames == null))
            {
                ValidationError error = new ValidationError("Please provide value to either field Column Indexes OR Column Names");
                metadata.AddValidationError(error);
            }

            base.CacheMetadata(metadata);
        }

        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                Status.Set(context, false);
                ErrorMsg.Set(context, string.Empty);

                DataTable origDT = InOutDataTable.Get(context); //Backup copy of Input DataTable

                InOutDataTable.Set(context, origDT); //In case of exception it will return Input DataTable

                DataTable InputDT = InOutDataTable.Get(context); //creating copy of Input DataTable

                List<string> ColNames = new List<string>(); //storing InputDT column names

                Console.WriteLine("**************");
                foreach (DataColumn col in InputDT.Columns)
                {
                    ColNames.Add(col.ColumnName);
                }

                //Console.WriteLine((ColumnIndexes.Get(context) != null).ToString());


                List<string> temporaryList = new List<string>();

                if (ColumnIndexes.Get(context) != null)
                {

                    foreach (int ColIndex in ColumnIndexes.Get(context))
                    {

                        if ((ColIndex < InputDT.Columns.Count) && (ColIndex >= 0))
                        {
                            Console.WriteLine(string.Format("ColumnName: {0} @ index: {1}", InputDT.Columns[ColIndex].ColumnName, ColIndex));
                            temporaryList.Add(InputDT.Columns[ColIndex].ColumnName);
                        }
                        else
                        {
                            ErrorMsg.Set(context, "Column Index value is more than number of columns in Input DataTable");
                            throw new Exception("Column Index value is more than number of columns in Input DataTable");
                        }
                    }

                    foreach (var col in temporaryList)
                    {
                        Console.WriteLine("Removing column: " + col);
                        try
                        {
                            InputDT.Columns.Remove(col);
                        }
                        catch (Exception e)
                        {
                            ErrorMsg.Set(context, string.Format("Unable to delete column: {0} due to: {1}", col, e.Message));
                            throw new Exception(string.Format("Unable to delete column: {0} due to: {1}", col, e.Message));
                        }

                    }
                }

                else
                {
                    foreach (string colName in ColumnNames.Get(context))
                    {
                        if (ColNames.Contains(colName.ToString()) == false)
                        {
                            ErrorMsg.Set(context, String.Format("{0} does NOT exist in Input DataTable", colName));
                            throw new Exception(String.Format("{0} does NOT exist in Input DataTable", colName));
                        }

                    }

                    foreach (var colName in ColumnNames.Get(context))
                    {
                        Console.WriteLine("Removing column: " + colName.ToString());
                        try
                        {
                            InputDT.Columns.Remove(colName.ToString());
                        }
                        catch (Exception e)
                        {
                            ErrorMsg.Set(context, string.Format("Unable to delete column: {0} due to: {1}", colName, e.Message));
                            throw new Exception(string.Format("Unable to delete column: {0} due to: {1}", colName.ToString(), e.Message));
                        }

                    }

                }


                InOutDataTable.Set(context, InputDT);
                Status.Set(context, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }
            
        }


        
    }


    public class GetSelectedColumns:CodeActivity
    {
        [Category("Input")]
        [DisplayName("Column Indexes")]
        [Description("Array of column indexes")]
        public InArgument<int[]> ColumnIndexes { get; set; }

        [Category("Input")]
        [DisplayName("Column Names")]
        [Description("list of column names to be Selected")]
        public InArgument<string[]> ColumnNames { get; set; }

        [Category("Input")]
        [RequiredArgument]
        [DisplayName("Input DataTable")]
        [Description("Input DataTable")]
        public InArgument<System.Data.DataTable> InputDataTable { get; set; }

        [Category("Output")]
        [RequiredArgument]
        [DisplayName("Output DataTable")]
        [Description("Output DataTable")]
        public OutArgument<System.Data.DataTable> OutputDataTable { get; set; }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {


            if ((ColumnIndexes != null) && (ColumnNames != null))
            {
                ValidationError error = new ValidationError("You can NOT give both fields: Column Indexes and Column Names values");
                metadata.AddValidationError(error);
            }

            if ((ColumnIndexes == null) && (ColumnNames == null))
            {
                ValidationError error = new ValidationError("Please provide value to either field Column Indexes OR Column Names");
                metadata.AddValidationError(error);
            }

            base.CacheMetadata(metadata);
        }


        protected override void Execute(CodeActivityContext context)
        {
            DataTable InputDT = InputDataTable.Get(context);

            if (ColumnIndexes.Get(context) != null)
            {
                List<string> temporaryList = new List<string>();

                foreach (int ColIndex in ColumnIndexes.Get(context))
                {

                    if ((ColIndex < InputDT.Columns.Count) && (ColIndex >= 0))
                    {
                        Console.WriteLine(string.Format("ColumnName: {0} @ index: {1}", InputDT.Columns[ColIndex].ColumnName, ColIndex));
                        temporaryList.Add(InputDT.Columns[ColIndex].ColumnName);
                    }
                    else
                    {
                        throw new Exception("Column Index value is more than number of columns in Input DataTable");
                    }
                }

                DataTable OutputDT = new DataView(InputDT).ToTable(false, temporaryList.ToArray());
                OutputDataTable.Set(context, OutputDT);


            }

            else
            {
                string[] SelectedColumns = ColumnNames.Get(context);

                List<string> ColNames = new List<string>();

                foreach (DataColumn col in InputDT.Columns)
                {
                    ColNames.Add(col.ColumnName);
                }

                foreach (var col in SelectedColumns)
                {
                    if (ColNames.Contains(col) == false)
                        throw new Exception(String.Format("{0} does NOT exist in Input DataTable", col));
                }

                DataTable OutputDT = new DataView(InputDT).ToTable(false, SelectedColumns);
                OutputDataTable.Set(context, OutputDT);
            }

            
        }
    }
}
