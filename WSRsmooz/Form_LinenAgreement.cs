﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using iTextSharp.text.pdf;

namespace WSRsmooz
{
    public partial class Form_LinenAgreement : Form
    {
        // 'global' variables
        public string client { get; set; }
        dbConnect database = new dbConnect();
        DataTable clientInfo;
        Random rng = new Random();

        // just pdf-y things
        PdfReader pdfReader;
        PdfStamper pdfStamper;
        AcroFields pdfFormFields;
        string templatePDF = "templates/7-LINEN AGREEMENT.pdf";
        string newFile;


        public void performReplacements()
        {
            Dictionary<string, string> changes = new Dictionary<string, string>();


            //
            //
            //
            //
            // PERFORM NAME SWAPS HERE!!!!!!!!!!!!!!!!!!!!!
            //
            // Both names must exist.
            // Template:
            //        changes.Add(DATABASE_NAME, PDF_EQUIVALENT);
            //
            //changes.Add("IntakeDate", "ADate");
            // changes.Add("SecPhone", "ClientWCM");
            //
            //
            //
            //
            //
            //
            //
            //



            foreach (KeyValuePair<string, string> change in changes)
            {
                clientInfo.Columns[change.Key].ColumnName = change.Value;
            }
        }

        public Form_LinenAgreement()
        {
            InitializeComponent();
        }

        private void Print_Click(object sender, EventArgs e)
        {
            if (!File.Exists(newFile))
            {
                pdfReader = new PdfReader(templatePDF);
            }
            else
            {
                MessageBox.Show("Error creating file.");
                return;
            }

            pdfStamper = new PdfStamper(pdfReader, new FileStream(newFile, FileMode.Create));
            pdfFormFields = pdfStamper.AcroFields;

            stampPdf();
            pdfStamper.Close();
            pdfReader.Close();
            printPdf();

            if (File.Exists(newFile))
                File.Delete(newFile);
        }

        // function to call printpdf.exe and wait for processing before deleting
        public void printPdf()
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "printpdf.exe";
            proc.StartInfo.Arguments = "-print-dialog -exit-on-print \"" + newFile + "\"";
            proc.StartInfo.RedirectStandardError = false;
            proc.StartInfo.RedirectStandardOutput = false;
            proc.StartInfo.UseShellExecute = true;
            proc.Start();
            proc.WaitForExit();
        }

        public void stampPdf()
        {
            // Fill from database.
            foreach (var field in pdfFormFields.Fields)
            {
                if (clientInfo.Columns.Contains(field.Key))
                {
                    pdfFormFields.SetField(field.Key, clientInfo.Rows[0][field.Key].ToString());
                }
            }

            // Traverse anything not in a group box.
            foreach (Control k in this.Controls)
            {
                if (k is TextBox || k is ComboBox || k is MaskedTextBox)
                {
                    pdfFormFields.SetField(k.Name, k.Text);
                }
                else if (k is DateTimePicker)
                {
                    pdfFormFields.SetField(k.Name, ((DateTimePicker)k).Value.Date.ToString("MM/dd/yyyy"));
                }
                else if (k is CheckBox)
                {
                    // Depends on "checked" value of PDF.
                    // Can modify later if anyone actually used a checkbox Acrofield.
                    if (((CheckBox)k).Checked)
                        pdfFormFields.SetField(k.Name, "X");
                }
            }

            // Traverse everything inside groupboxes.
            foreach (Control i in this.Controls.OfType<GroupBox>())
            {
                foreach (Control j in i.Controls)
                {
                    if (j is TextBox || j is ComboBox)
                    {
                        pdfFormFields.SetField(j.Name, j.Text);
                    }
                    else if (j is DateTimePicker)
                    {
                        pdfFormFields.SetField(j.Name, ((DateTimePicker)j).Value.Date.ToString("MM/dd/yyyy"));
                    }
                    else if (j is CheckBox)
                    {
                        // Depends on "checked" value of PDF.
                        // Can modify later if anyone actually used a checkbox Acrofield.
                        if (((CheckBox)j).Checked)
                            pdfFormFields.SetField(j.Name, "X");
                    }
                }
            }
        }




        private void Cancel_Click(object sender, EventArgs e)
        {
            String message = ("Are you sure you want to cancel? All unsaved progress will be lost");
            string caption = "Form Closing";
            var result = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void Form_LinenAgreement_Load(object sender, EventArgs e)
        {
            // create an obfuscated pdf
            newFile = Convert.ToString(rng.Next(int.MaxValue)) + ".pdf";

            client = "3";
            string query = "select * from ClientInfo where ClientID=" + client;

            clientInfo = database.GetTable(query);
            performReplacements();
        }


    }
}
