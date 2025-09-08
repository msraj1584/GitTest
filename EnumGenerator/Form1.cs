using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EnumGenerator
{
    public partial class Form1 : Form
    {

        string defaultnamespaceName = "DIO";
        string defaultclassName = "DIDO";
        public Form1()
        {
            InitializeComponent();

            txtNamespace.Text = defaultnamespaceName;
            txtClass.Text = defaultclassName;
        }

        private void btnBrowseCsv_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtCsvPath.Text = openFileDialog.FileName;
                }
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            //using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            //{
            //    saveFileDialog.Filter = "C# Files (*.cs)|*.cs";
            //    saveFileDialog.FileName = "DIDOEnums.cs";
            //    if (saveFileDialog.ShowDialog() == DialogResult.OK)
            //    {
            //        txtOutputPath.Text = saveFileDialog.FileName;
            //    }
            //}

            // for folder only option
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Select output folder";
                folderBrowserDialog.ShowNewFolderButton = true;

                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    // Fix filename as DIDOEnums.cs inside the chosen folder
                    string outputPath = System.IO.Path.Combine(folderBrowserDialog.SelectedPath, "DIDOEnums.cs");
                    txtOutputPath.Text = outputPath;
                }
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                string csvPath = txtCsvPath.Text;
                string outputPath = txtOutputPath.Text;

                string namespaceName = string.IsNullOrWhiteSpace(txtNamespace.Text) ? defaultnamespaceName : txtNamespace.Text.Trim();
                string className = string.IsNullOrWhiteSpace(txtClass.Text) ? defaultclassName : txtClass.Text.Trim();

                if (!File.Exists(csvPath))
                {
                    MessageBox.Show("Please select a valid CSV file.");
                    return;
                }

                var lines = File.ReadAllLines(csvPath)
                                .Skip(1) // skip header
                                .Select(l => l.Split(','))
                                .ToList();

                var inputs = lines.Where(l => l[1].Equals("Input", StringComparison.OrdinalIgnoreCase))
                                  .Select(l => new { Name = l[5].Trim(), Id = l[4].Trim(), IoName = l[6].Trim() })
                                  .ToList();

                var outputs = lines.Where(l => l[1].Equals("Output", StringComparison.OrdinalIgnoreCase))
                                   .Select(l => new { Name = l[5].Trim(), Id = l[4].Trim(), IoName = l[6].Trim() })
                                   .ToList();

                using (StreamWriter writer = new StreamWriter(outputPath))
                {
                    writer.WriteLine($"namespace {namespaceName}");
                    writer.WriteLine("{");
                    writer.WriteLine($"\tpublic partial class {className}");
                    writer.WriteLine("\t{");


                    // EInput
                    if (inputs.Any())
                    {
                        writer.WriteLine("\t\tpublic enum EInput");
                        writer.WriteLine("\t\t{");
                        writer.WriteLine("\t\t\tStart = -1,");

                        //foreach (var inp in inputs)
                        //    writer.WriteLine($"\t\t\t{inp.Name} = {inp.Id}, // {inp.IoName}");


                        foreach (var inp in inputs)
                        {
                            if (int.TryParse(inp.Id, out int idValue))
                                writer.WriteLine($"\t\t\t{inp.Name} = 0x{idValue:X4}, // {inp.IoName}");  // 🔹 padded HEX
                            else
                                writer.WriteLine($"\t\t\t{inp.Name} = -1, // {inp.IoName} (Invalid ID)");
                        }


                        writer.WriteLine("\t\t\tEnd");
                        writer.WriteLine("\t\t}");
                        writer.WriteLine();
                    }

                    // EOutput
                    if (outputs.Any())
                    {
                        writer.WriteLine("\t\tpublic enum EOutput");
                        writer.WriteLine("\t\t{");
                        writer.WriteLine("\t\t\tStart = -1,");

                        //foreach (var outp in outputs)
                        //    writer.WriteLine($"\t\t\t{outp.Name} = {outp.Id}, // {outp.IoName}");
                        foreach (var outp in outputs)
                        {
                            if (int.TryParse(outp.Id, out int idValue))
                                writer.WriteLine($"\t\t\t{outp.Name} = 0x{idValue:X4}, // {outp.IoName}");  // 🔹 padded HEX
                            else
                                writer.WriteLine($"\t\t\t{outp.Name} = -1, // {outp.IoName} (Invalid ID)");
                        }

                        writer.WriteLine("\t\t\tEnd");
                        writer.WriteLine("\t\t}");
                        writer.WriteLine();
                    }

                    writer.WriteLine("\t}");
                    writer.WriteLine("}");
                }


                MessageBox.Show("C# enum file generated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}
