using System.Windows.Forms;
using OpenNETCF.MTConnect;
using System.IO;
using System.Text;
using System;

namespace SampleClient
{
    public partial class MainForm
    {
        private const string DataFilePath = "C:\\mtc_data.csv";
        private StreamWriter m_dataFile;
        private object m_fileSyncRoot = new object();

        private void InitializeRecordData()
        {
            recordData.CheckedChanged += new System.EventHandler(recordData_CheckedChanged);
        }

        private void WriteLineToFile(string fileLine)
        {
            lock (m_fileSyncRoot)
            {
                if (m_dataFile != null)
                {
                    m_dataFile.Write(fileLine);
                }
            }
        }

        void recordData_CheckedChanged(object sender, System.EventArgs e)
        {
            bool startRecording = recordData.Checked;

            lock (m_fileSyncRoot)
            {
                if (startRecording)
                {
                    if (File.Exists(DataFilePath))
                    {
                        try
                        {
                            File.Delete(DataFilePath);
                        }
                        catch
                        {
                            MessageBox.Show("Unable to delete the existing file.  Close it and try again");
                            recordData.Checked = false;
                            return;
                        }
                    }

                    m_dataFile = File.CreateText(DataFilePath);

                    var headers = new StringBuilder();
                    for (int i = 0; i < dataList.Items.Count; i++)
                    {
                        var lvi = dataList.Items[i];
                        var columnName = ((DataItem)lvi.Tag).Name;
                        headers.AppendFormat("{0}{1}", columnName, i < (dataList.Items.Count - 1) ? "," : Environment.NewLine);
                    }

                    m_dataFile.Write(headers);
                }
                else
                {
                    if (m_dataFile != null)
                    {
                        m_dataFile.Close();
                        m_dataFile.Dispose();
                        m_dataFile = null;
                    }
                }
            }
        }
    }
}
