namespace EsfEditor.Forms
{
    using EsfEditor.Core.Enums;
    using EsfEditor.Core.EsfObjects;
    using EsfEditor.EditorObjects;
    using EsfEditor.Parser;
    using EsfEditor.Properties;
    using EsfEditor.Utils;
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class EsfEditorForm : Form
    {
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem cloneFromDiskToolStripMenuItem;
        private ToolStripMenuItem closeAllToolStripMenuItem;
        private ToolStripMenuItem closeToolStripMenuItem;
        private IContainer components;
        private ContextMenuStrip contextMenuStripDataGrid;
        private ContextMenuStrip contextMenuStripTreeView;
        private TreeNode copiedNode;
        private ToolStripMenuItem copyToolStripMenuItem;
        private DataGridView dataGrid;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private ToolStripMenuItem exportAsBinaryFileToolStripMenuItem;
        private BackgroundWorker ExportBackgroundWorker;
        private ToolStripMenuItem exportToolStripMenuItem;
        private ToolStripMenuItem fileToolStripMenuItem;
        private string filterAll = "All files|*.*";
        private string filterEsf = "ETW ESF files|*.esf";
        private string filterSaves = "ETW Savegames|*.empire_save";
        private ToolStripMenuItem helpToolStripMenuItem;
        private BindingSource iEsfValueBindingSource;
        private ToolStripMenuItem importFromBinaryFileToolStripMenuItem;
        private ToolStripMenuItem importToolStripMenuItem;
        private bool isBusy;
        private MenuStrip menuStrip;
        private string nodePath;
        private OpenFiles openedFiles = new OpenFiles();
        private ToolStripMenuItem openESFToolStripMenuItem;
        private OpenFileDialog openFileDialog;
        private ToolStripMenuItem openSavegameToolStripMenuItem;
        private ToolStripMenuItem pasteToolStripMenuItem;
        private TextBox pathText;
        private bool pathTextFocused;
        private ToolStripMenuItem quitToolStripMenuItem;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        private BackgroundWorker SaveEsfBackgroundWorker;
        private SaveFileDialog saveFileDialog;
        private ToolStripMenuItem saveToolStripMenuItem;
        private SplitContainer splitContainer1;
        private ToolStripStatusLabel statusLabel;
        private StatusStrip statusStrip;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripSeparator toolStripMenuItem4;
        private ToolStripSeparator toolStripMenuItem5;
        private ToolStripProgressBar toolStripProgressBar;
        private ToolStripSeparator toolStripSeparator1;
        private TreeView treeView;
        private ToolStripMenuItem undoCloneToolStripMenuItem;
        private ToolStripMenuItem undoDeleteToolStripMenuItem;
        private DataGridViewTextBoxColumn descriptionColumn;
        private DataGridViewTextBoxColumn valueColumn;
        private DataGridViewTextBoxColumn originalValueColumn;
        private DataGridViewTextBoxColumn typeColumn;
        private DataGridViewTextBoxColumn offsetColumn;
        private DataGridViewTextBoxColumn parentColumn;
        private DataGridViewTextBoxColumn isNewColumn;
        private DataGridViewTextBoxColumn isDeletedColumn;
        private ToolStripMenuItem undoToolStripMenuItem;

        public EsfEditorForm()
        {
            this.InitializeComponent();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutForm().ShowDialog();
        }

        private void AddCloseMenuItem(string fileName)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(fileName);
            item.Click += new EventHandler(this.closeToolStripMenuItem_Click);
            this.closeToolStripMenuItem.DropDownItems.Add(item);
            if (this.closeToolStripMenuItem.DropDownItems.Count > 1)
            {
                this.closeToolStripMenuItem.Visible = true;
            }
        }

        private void AddSaveMenuItem(string fileName, string filePath)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(fileName)
            {
                Tag = filePath
            };
            item.Click += new EventHandler(this.saveToolStripMenuItem_Click);
            this.saveToolStripMenuItem.DropDownItems.Add(item);
            ToolStripMenuItem item2 = new ToolStripMenuItem(fileName)
            {
                Tag = filePath
            };
            item2.Click += new EventHandler(this.saveAsToolStripMenuItem_Click);
            this.saveAsToolStripMenuItem.DropDownItems.Add(item2);
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e.Argument == null)
            {
                e.Cancel = true;
            }
            else
            {
                SaveObject argument = (SaveObject)e.Argument;
                if (argument.SaveAsFilePath == "")
                {
                    argument.File.Parser.Save(this.SaveEsfBackgroundWorker);
                    e.Result = argument.File;
                }
                else
                {
                    argument.File.Parser.SaveAs(argument.SaveAsFilePath, this.SaveEsfBackgroundWorker);
                }
            }
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.statusLabel.Text = "Saving " + e.ProgressPercentage + "%";
            this.toolStripProgressBar.Value = e.ProgressPercentage;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                this.statusLabel.Text = "Saved";
                if (e.Result != null)
                {
                    this.ReloadFile((EsfFile)e.Result);
                }
            }
            this.toolStripProgressBar.Value = 0;
            this.dataGrid.Refresh();
            this.SetBusy(false);
        }

        private void ClearEditor()
        {
            this.treeView.BeginUpdate();
            this.treeView.Nodes.Clear();
            this.treeView.EndUpdate();
            this.iEsfValueBindingSource.DataSource = null;
            this.pathText.Text = "";
            this.statusLabel.Text = "";
            this.closeToolStripMenuItem.DropDownItems.Clear();
            this.saveToolStripMenuItem.DropDownItems.Clear();
            this.saveAsToolStripMenuItem.DropDownItems.Clear();
        }

        private void ClearEditor(string removedFileName)
        {
            this.treeView.BeginUpdate();
            TreeNode node = null;
            foreach (TreeNode node2 in this.treeView.Nodes)
            {
                if (Path.GetFileName(node2.Text) == removedFileName)
                {
                    node = node2;
                }
            }
            if (node != null)
            {
                this.treeView.Nodes.Remove(node);
            }
            this.treeView.EndUpdate();
        }

        private void cloneFromDiskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = this.treeView.SelectedNode;
            selectedNode.Text = ((IEsfNode)selectedNode.Tag).Name;
            IEsfNode tag = (IEsfNode)selectedNode.Tag;            
            TreeNode parent = selectedNode.Parent;
            parent.Text = ((IEsfNode)parent.Tag).Name;
            IEsfNode node3 = tag.CopyTo((IEsfNode)parent.Tag, this.treeView.SelectedNode.Index + 1);
            TreeNode node = new TreeNode(node3.Name)
            {
                Tag = node3
            };
            parent.Nodes.Insert(this.treeView.SelectedNode.Index + 1, node);
            foreach (IEsfNode node6 in node3.GetChildren())
            {
                node.Nodes.Add(node6.Name).Tag = node6;
            }
            this.SetNodeColorChildren(node);
            this.SetNodeColorParents(parent);
            parent.Text = ((IEsfNode)parent.Tag).Title;
            selectedNode.Text = ((IEsfNode)selectedNode.Tag).Title;
            //TitleParser tP = new TitleParser();
            //List<IEsfNode> children = new List<IEsfNode>();
            //node.Text = tP.GetTitle((IEsfNode)node, out children);
        }

        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.openedFiles.UnregisterAllFile();
            this.ClearEditor();
        }

        private void CloseFile(string fileName)
        {
            this.openedFiles.UnregisterFile(fileName);
            this.RemoveCloseMenuItem(fileName);
            this.RemoveSaveMenuItem(fileName);
            this.ClearEditor(fileName);
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string text = ((ToolStripMenuItem)sender).Text;
            this.CloseFile(text);
        }

        private void contextMenuStripDataGrid_Opening(object sender, CancelEventArgs e)
        {
            Point point = this.dataGrid.PointToClient(new Point(Control.MousePosition.X, Control.MousePosition.Y));
            DataGridView.HitTestInfo info = this.dataGrid.HitTest(point.X, point.Y);
            if (info.RowIndex != -1)
            {
                DataGridViewRow row = this.dataGrid.Rows[info.RowIndex];
                IEsfValue dataBoundItem = (IEsfValue)row.DataBoundItem;
                this.dataGrid.ClearSelection();
                row.Cells[info.ColumnIndex].Selected = true;
                if (dataBoundItem.OriginalValue == null)
                {
                    this.undoToolStripMenuItem.Enabled = false;
                }
                else
                {
                    this.undoToolStripMenuItem.Enabled = true;
                }
                if (dataBoundItem.IsBinaryType)
                {
                    this.exportAsBinaryFileToolStripMenuItem.Visible = true;
                }
                else
                {
                    this.exportAsBinaryFileToolStripMenuItem.Visible = false;
                }
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.copiedNode = this.treeView.SelectedNode;
            ((IEsfNode)this.copiedNode.Tag).ParseDeep();
        }

        private void dataGrid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (this.dataGrid.Columns[e.ColumnIndex].Name == "valueColumn")
            {
                IEsfValue dataBoundItem = (IEsfValue)this.dataGrid.Rows[e.RowIndex].DataBoundItem;
                if (dataBoundItem.IsBinaryType)
                {
                    e.Cancel = true;
                    this.dataGrid.Rows[e.RowIndex].Cells["valueColumn"].ReadOnly = true;
                }
            }
        }

        private void dataGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (this.dataGrid.Columns[e.ColumnIndex].Name == "valueColumn")
            {
                DataGridViewRow row = this.dataGrid.Rows[e.RowIndex];
                IEsfValue dataBoundItem = (IEsfValue)row.DataBoundItem;
                if (row.Tag != null)
                {
                    if (((dataBoundItem.Type == EsfValueType.Ascii) || (dataBoundItem.Type == EsfValueType.UTF16)) && (dataBoundItem.Value == null))
                    {
                        dataBoundItem.Value = "";
                    }
                    if (row.Tag.ToString() != dataBoundItem.Value.ToString())
                    {
                        dataBoundItem.ChangeValue(row.Tag.ToString());
                        for (TreeNode node = this.treeView.SelectedNode; node.Parent != null; node = node.Parent)
                        {
                            this.SetNodeColor(node);
                        }
                    }
                }
            }
            else if (this.dataGrid.Columns[e.ColumnIndex].Name == "descriptionColumn")
            {
                DataGridViewRow row2 = this.dataGrid.Rows[e.RowIndex];
                IEsfValue value3 = (IEsfValue)row2.DataBoundItem;
                NodesStructure.SetNodeDescriptions(value3.Parent);
            }
        }

        private void dataGrid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if ((!this.dataGrid.CurrentRow.Cells["valueColumn"].ReadOnly && (e.FormattedValue.ToString() != "System.Byte[]")) && (this.dataGrid.Columns[e.ColumnIndex].Name == "valueColumn"))
            {
                DataGridViewRow row = this.dataGrid.Rows[e.RowIndex];
                IEsfValue dataBoundItem = (IEsfValue)row.DataBoundItem;
                string str = ((IEsfValue)this.dataGrid.CurrentRow.DataBoundItem).Value.ToString();
                if (dataBoundItem.ValidateNewValue(e.FormattedValue.ToString()))
                {
                    this.dataGrid.Rows[e.RowIndex].Tag = str;
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((EsfNode)this.treeView.SelectedNode.Tag).Delete();
            this.SetNodeColorParents(this.treeView.SelectedNode.Parent);
            this.SetNodeColorChildren(this.treeView.SelectedNode);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void EsfEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.isBusy)
            {
                e.Cancel = true;
            }
            else
            {
                if (NodesStructure.NeedSave)
                {
                    NodesStructure.Save(SerializationType.Xml);
                }
                foreach (EsfFile file in this.openedFiles.EsfFiles)
                {
                    if (file.HasChanges() && (MessageBox.Show(this, "File " + Path.GetFileName(file.FilePath) + " has unsaved changes. Do you want the opportunity to save them?", "Unsaved changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes))
                    {
                        e.Cancel = true;
                    }
                }
                this.openedFiles.Dispose();
                this.SaveFormData();
            }
        }

        private void EsfEditorForm_Load(object sender, EventArgs e)
        {
            this.RestoreFormData();
            NodesStructure.LoadNodesDescription(SerializationType.Xml);
        }

        private void exportAsBinaryFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IEsfValue dataBoundItem = (IEsfValue)this.dataGrid.SelectedCells[0].OwningRow.DataBoundItem;
            this.saveFileDialog.InitialDirectory = Path.GetDirectoryName(this.openFileDialog.FileName);
            if (this.saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                SaveFileHelper.SaveBinaryFile(this.saveFileDialog.FileName, (byte[])dataBoundItem.Value);
            }
        }

        private void ExportBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e.Argument == null)
            {
                e.Cancel = true;
            }
            else
            {
                ExportObject argument = (ExportObject)e.Argument;
                argument.node.Parser.SaveExport(argument.fileName, argument.node, this.ExportBackgroundWorker);
            }
        }

        private void ExportBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.statusLabel.Text = "Exporting " + e.ProgressPercentage + "%";
            this.toolStripProgressBar.Value = e.ProgressPercentage;
        }

        private void ExportBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.statusLabel.Text = " Export file " + Path.GetFileName(this.saveFileDialog.FileName) + " saved";
            this.toolStripProgressBar.Value = 0;
            this.SetBusy(false);
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IEsfNode tag = (IEsfNode)this.treeView.SelectedNode.Tag;
            this.saveFileDialog.DefaultExt = "esf";
            this.saveFileDialog.Filter = this.filterEsf;
            this.saveFileDialog.InitialDirectory = Path.GetDirectoryName(this.openFileDialog.FileName);
            if (this.saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                this.SetBusy(true);
                this.ExportBackgroundWorker.RunWorkerAsync(new ExportObject(this.saveFileDialog.FileName, tag));
            }
        }

        private void importFromBinaryFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.exportAsBinaryFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.SaveEsfBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openESFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openSavegameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripTreeView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cloneFromDiskToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.undoCloneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoDeleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripDataGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importFromBinaryFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pathText = new System.Windows.Forms.TextBox();
            this.dataGrid = new System.Windows.Forms.DataGridView();
            this.iEsfValueBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.treeView = new System.Windows.Forms.TreeView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ExportBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.isDeletedColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.isNewColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.parentColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.offsetColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.typeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.originalValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.valueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.descriptionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.menuStrip.SuspendLayout();
            this.contextMenuStripTreeView.SuspendLayout();
            this.contextMenuStripDataGrid.SuspendLayout();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.iEsfValueBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // exportAsBinaryFileToolStripMenuItem
            // 
            this.exportAsBinaryFileToolStripMenuItem.Name = "exportAsBinaryFileToolStripMenuItem";
            this.exportAsBinaryFileToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.exportAsBinaryFileToolStripMenuItem.Text = "Export as binary file";
            this.exportAsBinaryFileToolStripMenuItem.Visible = false;
            this.exportAsBinaryFileToolStripMenuItem.Click += new System.EventHandler(this.exportAsBinaryFileToolStripMenuItem_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Empire: Total War - ESF files|*.esf";
            // 
            // SaveEsfBackgroundWorker
            // 
            this.SaveEsfBackgroundWorker.WorkerReportsProgress = true;
            this.SaveEsfBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_DoWork);
            this.SaveEsfBackgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker_ProgressChanged);
            this.SaveEsfBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_RunWorkerCompleted);
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(799, 24);
            this.menuStrip.TabIndex = 18;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openESFToolStripMenuItem,
            this.openSavegameToolStripMenuItem,
            this.closeToolStripMenuItem,
            this.closeAllToolStripMenuItem,
            this.toolStripMenuItem4,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripMenuItem5,
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openESFToolStripMenuItem
            // 
            this.openESFToolStripMenuItem.Name = "openESFToolStripMenuItem";
            this.openESFToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.openESFToolStripMenuItem.Text = "Open ESF";
            this.openESFToolStripMenuItem.Click += new System.EventHandler(this.openESFToolStripMenuItem_Click);
            // 
            // openSavegameToolStripMenuItem
            // 
            this.openSavegameToolStripMenuItem.Name = "openSavegameToolStripMenuItem";
            this.openSavegameToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.openSavegameToolStripMenuItem.Text = "Open Savegame";
            this.openSavegameToolStripMenuItem.Click += new System.EventHandler(this.openSavegameToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Visible = false;
            // 
            // closeAllToolStripMenuItem
            // 
            this.closeAllToolStripMenuItem.Name = "closeAllToolStripMenuItem";
            this.closeAllToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.closeAllToolStripMenuItem.Text = "Close All";
            this.closeAllToolStripMenuItem.Click += new System.EventHandler(this.closeAllToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(157, 6);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.saveToolStripMenuItem.Text = "Save";
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.saveAsToolStripMenuItem.Text = "Save as ...";
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(157, 6);
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.quitToolStripMenuItem.Text = "Quit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.helpToolStripMenuItem.Text = "About";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // contextMenuStripTreeView
            // 
            this.contextMenuStripTreeView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.cloneFromDiskToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.toolStripMenuItem1,
            this.undoCloneToolStripMenuItem,
            this.undoDeleteToolStripMenuItem,
            this.toolStripSeparator1,
            this.importToolStripMenuItem,
            this.exportToolStripMenuItem});
            this.contextMenuStripTreeView.Name = "contextMenuStripShort";
            this.contextMenuStripTreeView.Size = new System.Drawing.Size(139, 192);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // cloneFromDiskToolStripMenuItem
            // 
            this.cloneFromDiskToolStripMenuItem.Name = "cloneFromDiskToolStripMenuItem";
            this.cloneFromDiskToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.cloneFromDiskToolStripMenuItem.Text = "Clone ";
            this.cloneFromDiskToolStripMenuItem.Click += new System.EventHandler(this.cloneFromDiskToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(135, 6);
            // 
            // undoCloneToolStripMenuItem
            // 
            this.undoCloneToolStripMenuItem.Name = "undoCloneToolStripMenuItem";
            this.undoCloneToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.undoCloneToolStripMenuItem.Text = "Undo copy";
            this.undoCloneToolStripMenuItem.Click += new System.EventHandler(this.undoCloneToolStripMenuItem_Click);
            // 
            // undoDeleteToolStripMenuItem
            // 
            this.undoDeleteToolStripMenuItem.Name = "undoDeleteToolStripMenuItem";
            this.undoDeleteToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.undoDeleteToolStripMenuItem.Text = "Undo delete";
            this.undoDeleteToolStripMenuItem.Click += new System.EventHandler(this.undoDeleteToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(135, 6);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.importToolStripMenuItem.Text = "Import";
            this.importToolStripMenuItem.Visible = false;
            this.importToolStripMenuItem.Click += new System.EventHandler(this.importToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.exportToolStripMenuItem.Text = "Export";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // contextMenuStripDataGrid
            // 
            this.contextMenuStripDataGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.importFromBinaryFileToolStripMenuItem,
            this.exportAsBinaryFileToolStripMenuItem});
            this.contextMenuStripDataGrid.Name = "contextMenuStripListView";
            this.contextMenuStripDataGrid.Size = new System.Drawing.Size(195, 70);
            this.contextMenuStripDataGrid.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripDataGrid_Opening);
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.undoToolStripMenuItem.Text = "Undo";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
            // 
            // importFromBinaryFileToolStripMenuItem
            // 
            this.importFromBinaryFileToolStripMenuItem.Name = "importFromBinaryFileToolStripMenuItem";
            this.importFromBinaryFileToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.importFromBinaryFileToolStripMenuItem.Text = "Import from binary file";
            this.importFromBinaryFileToolStripMenuItem.Visible = false;
            this.importFromBinaryFileToolStripMenuItem.Click += new System.EventHandler(this.importFromBinaryFileToolStripMenuItem_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.toolStripProgressBar});
            this.statusStrip.Location = new System.Drawing.Point(0, 515);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(799, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 19;
            this.statusStrip.Text = "statusStrip";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(682, 17);
            this.statusLabel.Spring = true;
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripProgressBar
            // 
            this.toolStripProgressBar.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripProgressBar.Name = "toolStripProgressBar";
            this.toolStripProgressBar.Size = new System.Drawing.Size(100, 16);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "Value";
            this.dataGridViewTextBoxColumn1.HeaderText = "Value";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "OriginalValue";
            this.dataGridViewTextBoxColumn2.HeaderText = "Original Value";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.DataPropertyName = "Value";
            this.dataGridViewTextBoxColumn3.HeaderText = "Value";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.DataPropertyName = "OriginalValue";
            this.dataGridViewTextBoxColumn4.HeaderText = "Original Value";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            // 
            // pathText
            // 
            this.pathText.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pathText.Location = new System.Drawing.Point(0, 459);
            this.pathText.Name = "pathText";
            this.pathText.ReadOnly = true;
            this.pathText.Size = new System.Drawing.Size(550, 20);
            this.pathText.TabIndex = 16;
            this.pathText.Enter += new System.EventHandler(this.pathText_Enter);
            this.pathText.Leave += new System.EventHandler(this.pathText_Leave);
            this.pathText.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pathText_MouseUp);
            // 
            // dataGrid
            // 
            this.dataGrid.AllowUserToAddRows = false;
            this.dataGrid.AllowUserToDeleteRows = false;
            this.dataGrid.AllowUserToResizeRows = false;
            this.dataGrid.AutoGenerateColumns = false;
            this.dataGrid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.descriptionColumn,
            this.valueColumn,
            this.originalValueColumn,
            this.typeColumn,
            this.offsetColumn,
            this.parentColumn,
            this.isNewColumn,
            this.isDeletedColumn});
            this.dataGrid.DataSource = this.iEsfValueBindingSource;
            this.dataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGrid.Location = new System.Drawing.Point(0, 0);
            this.dataGrid.Name = "dataGrid";
            this.dataGrid.RowHeadersVisible = false;
            this.dataGrid.Size = new System.Drawing.Size(550, 459);
            this.dataGrid.TabIndex = 17;
            this.dataGrid.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dataGrid_CellBeginEdit);
            this.dataGrid.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGrid_CellEndEdit);
            this.dataGrid.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dataGrid_CellValidating);
            // 
            // iEsfValueBindingSource
            // 
            this.iEsfValueBindingSource.AllowNew = false;
            this.iEsfValueBindingSource.DataSource = typeof(IEsfValue);
            // 
            // treeView
            // 
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(213, 479);
            this.treeView.TabIndex = 0;
            this.treeView.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterExpand);
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
            this.treeView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.treeView_MouseClick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 27);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dataGrid);
            this.splitContainer1.Panel2.Controls.Add(this.pathText);
            this.splitContainer1.Size = new System.Drawing.Size(767, 479);
            this.splitContainer1.SplitterDistance = 213;
            this.splitContainer1.TabIndex = 16;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.DataPropertyName = "Value";
            this.dataGridViewTextBoxColumn5.HeaderText = "Value";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.DataPropertyName = "OriginalValue";
            this.dataGridViewTextBoxColumn6.HeaderText = "Original Value";
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            this.dataGridViewTextBoxColumn6.ReadOnly = true;
            // 
            // ExportBackgroundWorker
            // 
            this.ExportBackgroundWorker.WorkerReportsProgress = true;
            this.ExportBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.ExportBackgroundWorker_DoWork);
            this.ExportBackgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.ExportBackgroundWorker_ProgressChanged);
            this.ExportBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.ExportBackgroundWorker_RunWorkerCompleted);
            // 
            // isDeletedColumn
            // 
            this.isDeletedColumn.DataPropertyName = "IsDeleted";
            this.isDeletedColumn.HeaderText = "IsDeleted";
            this.isDeletedColumn.Name = "isDeletedColumn";
            this.isDeletedColumn.Visible = false;
            // 
            // isNewColumn
            // 
            this.isNewColumn.DataPropertyName = "IsNew";
            this.isNewColumn.HeaderText = "IsNew";
            this.isNewColumn.Name = "isNewColumn";
            this.isNewColumn.Visible = false;
            // 
            // parentColumn
            // 
            this.parentColumn.DataPropertyName = "Parent";
            this.parentColumn.HeaderText = "Parent";
            this.parentColumn.Name = "parentColumn";
            this.parentColumn.Visible = false;
            // 
            // offsetColumn
            // 
            this.offsetColumn.DataPropertyName = "Offset";
            this.offsetColumn.HeaderText = "Offset";
            this.offsetColumn.Name = "offsetColumn";
            this.offsetColumn.ReadOnly = true;
            this.offsetColumn.Visible = false;
            // 
            // typeColumn
            // 
            this.typeColumn.DataPropertyName = "Type";
            this.typeColumn.HeaderText = "Type";
            this.typeColumn.Name = "typeColumn";
            this.typeColumn.ReadOnly = true;
            // 
            // originalValueColumn
            // 
            this.originalValueColumn.DataPropertyName = "OriginalValue";
            this.originalValueColumn.HeaderText = "Original Value";
            this.originalValueColumn.Name = "originalValueColumn";
            this.originalValueColumn.ReadOnly = true;
            // 
            // valueColumn
            // 
            this.valueColumn.ContextMenuStrip = this.contextMenuStripDataGrid;
            this.valueColumn.DataPropertyName = "Value";
            this.valueColumn.HeaderText = "Value";
            this.valueColumn.Name = "valueColumn";
            // 
            // descriptionColumn
            // 
            this.descriptionColumn.DataPropertyName = "Description";
            this.descriptionColumn.HeaderText = "Description";
            this.descriptionColumn.Name = "descriptionColumn";
            // 
            // EsfEditorForm
            // 
            this.ClientSize = new System.Drawing.Size(799, 537);
            this.Controls.Add(this.menuStrip);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.splitContainer1);
            this.Location = new System.Drawing.Point(100, 100);
            this.MinimumSize = new System.Drawing.Size(540, 400);
            this.Name = "EsfEditorForm";
            this.ShowIcon = false;
            this.Text = "ESF Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EsfEditor_FormClosing);
            this.Load += new System.EventHandler(this.EsfEditorForm_Load);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.contextMenuStripTreeView.ResumeLayout(false);
            this.contextMenuStripDataGrid.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.iEsfValueBindingSource)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void openESFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.openFileDialog.Filter = this.filterEsf + "|" + this.filterSaves + "|" + this.filterAll;
            string str = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null);
            if (str == null)
            {
                str = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam", "InstallPath", null);
            }
            if (str != null)
            {
                this.openFileDialog.InitialDirectory = Path.Combine(str, @"steamapps\common\empire total war\data");
            }
            this.OpenFile();
        }

        private void OpenFile()
        {
            treeView.BeginUpdate();
            if (this.openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                if (!this.openedFiles.IsOpened(this.openFileDialog.FileName))
                {
                    EsfFile file = new EsfFile(this.openFileDialog.FileName);
                    try
                    {
                        this.openedFiles.RegisterFile(file);
                        TreeNode node = this.treeView.Nodes.Add(Path.GetFileName(Path.GetDirectoryName(this.openFileDialog.FileName)) + Path.DirectorySeparatorChar + Path.GetFileName(this.openFileDialog.FileName));
                        foreach (IEsfNode node2 in file.RootNodes)
                        {
                            node.Nodes.Add(node2.Name).Tag = node2;
                        }
                        this.statusLabel.Text = "Opened " + this.openFileDialog.FileName;
                        this.AddCloseMenuItem(file.FileName);
                        this.AddSaveMenuItem(file.FileName, file.FilePath);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("Parse failed!" + Environment.NewLine + Environment.NewLine + exception.Message + Environment.NewLine + exception.StackTrace);
                        this.openedFiles.UnregisterFile(file);
                    }
                }
                else
                {
                    MessageBox.Show("File " + Path.GetFileName(this.openFileDialog.FileName) + " already open", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            treeView.EndUpdate();
        }

            private void OpenFile(string filePath)
        {
            treeView.BeginUpdate();
            EsfFile file = new EsfFile(filePath);
            try
            {
                this.openedFiles.RegisterFile(file);
                TreeNode node = this.treeView.Nodes.Add(Path.GetFileName(Path.GetDirectoryName(this.openFileDialog.FileName)) + Path.DirectorySeparatorChar + Path.GetFileName(this.openFileDialog.FileName));
                foreach (IEsfNode node2 in file.RootNodes)
                {
                    node.Nodes.Add(node2.Name).Tag = node2;
                }
                this.AddCloseMenuItem(file.FileName);
                this.AddSaveMenuItem(file.FileName, file.FilePath);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Parse failed!" + Environment.NewLine + Environment.NewLine + exception.Message + Environment.NewLine + exception.StackTrace);
                this.openedFiles.UnregisterFile(file);
            }
            treeView.EndUpdate();
        }

        private void openSavegameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.openFileDialog.Filter = this.filterSaves + "|" + this.filterEsf + "|" + this.filterAll;
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            this.openFileDialog.InitialDirectory = Path.Combine(folderPath, @"The Creative Assembly\Empire\save_games");
            this.OpenFile();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode parent;
            int num;
            TreeNode selectedNode = this.treeView.SelectedNode;
            selectedNode.Text = ((IEsfNode)selectedNode.Tag).Name; 
            this.copiedNode.Text  = ((IEsfNode)this.copiedNode.Tag).Name;
            IEsfNode tag = (IEsfNode)this.copiedNode.Tag;
            IEsfNode destination = (IEsfNode)selectedNode.Tag;
            if (tag is EsfPolyNode)
            {
                if (destination.Type == EsfValueType.PolyNode)
                {
                    parent = selectedNode;
                    destination = (IEsfNode)parent.Tag;
                    num = 0;
                }
                else
                {
                    parent = selectedNode.Parent;
                    parent.Text = ((IEsfNode)parent.Tag).Name;
                    destination = (IEsfNode)parent.Tag;
                    num = selectedNode.Index + 1;
                }
            }
            else
            {
                parent = selectedNode;
                destination = (IEsfNode)parent.Tag;
                num = 0;
            }
            IEsfNode node3 = tag.CopyTo(destination, num);
            TreeNode node = new TreeNode(node3.Name)
            {
                Tag = node3
            };
            parent.Nodes.Insert(num, node);
            foreach (IEsfNode node7 in node3.GetChildren())
            {
                node.Nodes.Add(node7.Name).Tag = node7;
            }
            this.SetNodeColorChildren(node);
            this.SetNodeColorParents(parent);
            parent.Text = ((IEsfNode)parent.Tag).Title;
            selectedNode.Text = ((IEsfNode)selectedNode.Tag).Title;
            //TitleParser tP = new TitleParser();
            //List<IEsfNode> children = new List<IEsfNode>();
            //node.Text = tP.GetTitle((IEsfNode)node, out children);
        }

        private void pathText_Enter(object sender, EventArgs e)
        {
            if (Control.MouseButtons == MouseButtons.None)
            {
                this.pathText.SelectAll();
            }
        }

        private void pathText_Leave(object sender, EventArgs e)
        {
            this.pathTextFocused = false;
        }

        private void pathText_MouseUp(object sender, MouseEventArgs e)
        {
            if (!this.pathTextFocused && (this.pathText.SelectionLength == 0))
            {
                this.pathTextFocused = true;
                this.pathText.SelectAll();
            }
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            base.Close();
        }

        private void ReloadFile(EsfFile file)
        {
            treeView.BeginUpdate();
            file.Reload();
            string str = Path.GetFileName(Path.GetDirectoryName(file.FilePath)) + Path.DirectorySeparatorChar + Path.GetFileName(file.FilePath);
            foreach (TreeNode node in this.treeView.Nodes)
            {
                if (node.Text == str)
                {
                    node.Nodes.Clear();
                    foreach (IEsfNode node2 in file.RootNodes)
                    {
                        node.Nodes.Add(node2.Name).Tag = node2;
                    }
                    this.SetNodeColorChildren(node);
                    node.Collapse();
                }
            }
            this.pathText.Text = "";
            this.iEsfValueBindingSource.DataSource = null;
            treeView.EndUpdate();
        }

        private void RemoveCloseMenuItem(string fileName)
        {
            ToolStripMenuItem item = null;
            foreach (ToolStripMenuItem item2 in this.closeToolStripMenuItem.DropDownItems)
            {
                if (item2.Text == fileName)
                {
                    item = item2;
                }
            }
            if (item != null)
            {
                this.closeToolStripMenuItem.DropDownItems.Remove(item);
            }
            if (this.closeToolStripMenuItem.DropDownItems.Count < 2)
            {
                this.closeToolStripMenuItem.Visible = false;
            }
        }

        private void RemoveSaveMenuItem(string fileName)
        {
            ToolStripMenuItem item = null;
            foreach (ToolStripMenuItem item2 in this.saveToolStripMenuItem.DropDownItems)
            {
                if (item2.Text == fileName)
                {
                    item = item2;
                }
            }
            if (item != null)
            {
                this.saveToolStripMenuItem.DropDownItems.Remove(item);
                item = null;
            }
            foreach (ToolStripMenuItem item3 in this.saveAsToolStripMenuItem.DropDownItems)
            {
                if (item3.Text == fileName)
                {
                    item = item3;
                }
            }
            if (item != null)
            {
                this.saveAsToolStripMenuItem.DropDownItems.Remove(item);
            }
        }

        private void RestoreFormData()
        {
            this.Text = this.Text + " " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            try
            {
                base.StartPosition = FormStartPosition.Manual;
                base.Size = Settings.Default.FormSize;
                base.Location = Settings.Default.FormLocation;
                base.WindowState = Settings.Default.FormState;
                StringCollection dataGridViewFormColumns = Settings.Default.DataGridViewFormColumns;
                string[] array = new string[dataGridViewFormColumns.Count];
                dataGridViewFormColumns.CopyTo(array, 0);
                Array.Sort<string>(array);
                for (int i = 0; i < array.Length; i++)
                {
                    string[] strArray2 = array[i].Split(new char[] { ',' });
                    int num2 = int.Parse(strArray2[0]);
                    this.dataGrid.Columns[num2].Width = short.Parse(strArray2[1]);
                }
            }
            catch (NullReferenceException)
            {
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string tag = (string)((ToolStripMenuItem)sender).Tag;
            EsfFile file = this.openedFiles.GetFile(tag);
            if (file != null)
            {
                if (Path.GetExtension(this.openFileDialog.FileName).Equals(".esf", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.saveFileDialog.DefaultExt = "esf";
                    this.saveFileDialog.Filter = this.filterEsf + "|" + this.filterSaves + "|" + this.filterAll;
                }
                else
                {
                    this.saveFileDialog.DefaultExt = "empire_save";
                    this.saveFileDialog.Filter = this.filterSaves + "|" + this.filterEsf + "|" + this.filterAll;
                }
                this.saveFileDialog.InitialDirectory = Path.GetDirectoryName(this.openFileDialog.FileName);
                if (this.saveFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        this.SetBusy(true);
                        this.SaveEsfBackgroundWorker.RunWorkerAsync(new SaveObject(file, this.saveFileDialog.FileName));
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("Save failed!" + Environment.NewLine + Environment.NewLine + exception.Message + Environment.NewLine + exception.StackTrace);
                    }
                }
            }
        }

        private void SaveFormData()
        {
            StringCollection strings = new StringCollection();
            int num = 0;
            foreach (DataGridViewColumn column in this.dataGrid.Columns)
            {
                strings.Add(string.Format("{0},{1}", num++, column.Width));
            }
            Settings.Default.DataGridViewFormColumns = strings;
            if (base.WindowState == FormWindowState.Normal)
            {
                Settings.Default.FormSize = base.Size;
            }
            else
            {
                Settings.Default.FormSize = base.RestoreBounds.Size;
            }
            Settings.Default.FormLocation = base.Location;
            Settings.Default.FormState = base.WindowState;
            Settings.Default.Save();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string tag = (string)((ToolStripMenuItem)sender).Tag;
            EsfFile file = this.openedFiles.GetFile(tag);
            if (file != null)
            {
                if (file.Parser.readOnly)
                {
                    MessageBox.Show(this, "This is read only file", "Info", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    try
                    {
                        this.SetBusy(true);
                        this.SaveEsfBackgroundWorker.RunWorkerAsync(new SaveObject(file, ""));
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("Save failed!" + Environment.NewLine + Environment.NewLine + exception.Message + Environment.NewLine + exception.StackTrace);
                    }
                }
            }
        }

        private void SetBusy(bool isBusy)
        {
            this.fileToolStripMenuItem.Enabled = !isBusy;
            this.dataGrid.ReadOnly = isBusy;
            this.treeView.Enabled = !isBusy;
            this.isBusy = isBusy;
        }

        private void SetNodeColor(TreeNode treeNode)
        {
            treeView.BeginUpdate();
            if (treeNode.Tag != null)
            {
                IEsfNode tag = (IEsfNode)treeNode.Tag;
                if (tag.Type == EsfValueType.PolyNode)
                {
                    treeNode.ForeColor = System.Drawing.Color.Gray;
                }
                else
                {
                    treeNode.ForeColor = this.treeView.ForeColor;
                }
                if ((tag.ContainsChanges > 0) && (tag.TreeContainsChanges > 0))
                {
                    treeNode.NodeFont = new Font(this.treeView.Font, FontStyle.Italic | FontStyle.Bold);
                }
                else if ((tag.ContainsChanges > 0) && (tag.TreeContainsChanges <= 0))
                {
                    treeNode.NodeFont = new Font(this.treeView.Font, FontStyle.Bold);
                }
                else if ((tag.ContainsChanges <= 0) && (tag.TreeContainsChanges > 0))
                {
                    treeNode.NodeFont = new Font(this.treeView.Font, FontStyle.Italic);
                }
                else if ((tag.ContainsChanges <= 0) && (tag.TreeContainsChanges <= 0))
                {
                    treeNode.NodeFont = new Font(this.treeView.Font, FontStyle.Regular);
                }
                if (tag.IsNew > 0)
                {
                    if (tag.Type == EsfValueType.PolyNode)
                    {
                        treeNode.ForeColor = System.Drawing.Color.DarkGreen;
                    }
                    else
                    {
                        treeNode.ForeColor = System.Drawing.Color.Green;
                    }
                }
                if (tag.IsDeleted > 0)
                {
                    if (tag.Type == EsfValueType.PolyNode)
                    {
                        treeNode.ForeColor = System.Drawing.Color.DarkRed;
                    }
                    else
                    {
                        treeNode.ForeColor = System.Drawing.Color.Red;
                    }
                }
                if (treeNode.NodeFont.Bold)
                {
                    string text = treeNode.Text;
                    treeNode.Text = text;
                }
            }
            treeView.EndUpdate();
        }

        private void SetNodeColorChildren(TreeNode treeNode)
        {
            treeView.BeginUpdate();
            this.SetNodeColor(treeNode);
            foreach (TreeNode node in treeNode.Nodes)
            {
                this.SetNodeColorChildren(node);
            }
            treeView.EndUpdate();
        }

        private void SetNodeColorParents(TreeNode treeNode)
        {
            treeView.BeginUpdate();
            while (treeNode.Parent != null)
            {
                this.SetNodeColor(treeNode);
                treeNode = treeNode.Parent;
            }
            treeView.EndUpdate();
        }

        private void treeView_AfterExpand(object sender, TreeViewEventArgs e)
        {
            treeView.BeginUpdate();
            foreach (TreeNode node in e.Node.Nodes)
            {
                List<IEsfNode> children = new List<IEsfNode>();
                //The CAI nodes don't have expanded titles yet 
                if (node.Text.StartsWith("CAI") != true)
                {
                    TitleParser tP = new TitleParser();
                    node.Text = tP.GetTitle((IEsfNode)node.Tag, out children);
                }
                else
                    children = ((IEsfNode)node.Tag).GetChildren();
                if ((node.Nodes.Count == 0) || (node.Nodes.Count != children.Count))
                {
                    node.Nodes.Clear();
                    foreach (IEsfNode node3 in children)
                    {
                        TreeNode treeNode = node.Nodes.Add(node3.Name);
                        treeNode.Tag = node3;
                        this.SetNodeColor(treeNode);
                    }
                    continue;
                }
            }
            treeView.EndUpdate();
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            treeView.BeginUpdate();
            this.nodePath = e.Node.FullPath;
            this.pathText.Text = this.nodePath;
            IEsfNode tag = (IEsfNode)e.Node.Tag;
            if (tag != null)
            {
                List<IEsfValue> values = tag.GetValues();
                Console.WriteLine("Values:");
                Console.WriteLine(values);
                NodesStructure.SetDescriptions(ref values, tag.Name);
                this.iEsfValueBindingSource.DataSource = values;
            }
            else
            {
                this.iEsfValueBindingSource.DataSource = null;
            }
            treeView.EndUpdate();
        }

        private void treeView_MouseClick(object sender, MouseEventArgs e)
        {
            treeView.BeginUpdate();
            if ((e.Button == MouseButtons.Right) && !this.isBusy)
            {
                TreeViewHitTestInfo info = this.treeView.HitTest(e.X, e.Y);
                info.Node.TreeView.SelectedNode = info.Node;
                TreeNode node = info.Node;
                this.treeView.Nodes.Contains(node);
                if (info.Node.Tag != null)
                {
                    IEsfNode tag = (IEsfNode)info.Node.Tag;
                    this.copyToolStripMenuItem.Enabled = true;
                    this.pasteToolStripMenuItem.Enabled = false;
                    this.cloneFromDiskToolStripMenuItem.Enabled = true;
                    this.deleteToolStripMenuItem.Enabled = true;
                    this.undoCloneToolStripMenuItem.Enabled = false;
                    this.undoDeleteToolStripMenuItem.Enabled = false;
                    if ((this.copiedNode != null) && (this.copiedNode.Name == info.Node.Name))
                    {
                        this.pasteToolStripMenuItem.Enabled = true;
                    }
                    if (tag.IsDeleted > 0)
                    {
                        if ((tag.Parent == null) || (tag.Parent.IsDeleted < tag.IsDeleted))
                        {
                            this.undoDeleteToolStripMenuItem.Enabled = true;
                        }
                        this.deleteToolStripMenuItem.Enabled = false;
                        this.cloneFromDiskToolStripMenuItem.Enabled = false;
                    }
                    if (tag.IsNew > 0)
                    {
                        if ((tag.Parent == null) || (tag.Parent.IsNew < tag.IsNew))
                        {
                            this.undoCloneToolStripMenuItem.Enabled = true;
                        }
                        this.deleteToolStripMenuItem.Enabled = false;
                    }
                    this.exportToolStripMenuItem.Enabled = true;
                    switch (tag.Type)
                    {
                        case EsfValueType.SingleNode:
                            this.cloneFromDiskToolStripMenuItem.Enabled = false;
                            break;

                        case EsfValueType.PolyNode:
                            this.cloneFromDiskToolStripMenuItem.Enabled = false;
                            break;

                        default:
                            this.exportToolStripMenuItem.Enabled = false;
                            break;
                    }
                    this.contextMenuStripTreeView.Show(this.treeView, e.X, e.Y);
                }
            }
            treeView.EndUpdate();
        }

        private void undoCloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = this.treeView.SelectedNode;
            ((IEsfNode)selectedNode.Tag).UndoCopy();
            this.SetNodeColorParents(selectedNode);
            selectedNode.Remove();
        }

        private void undoDeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = this.treeView.SelectedNode;
            ((IEsfNode)selectedNode.Tag).UnDelete();
            this.SetNodeColorParents(selectedNode.Parent);
            this.SetNodeColorChildren(selectedNode);
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((IEsfValue)this.dataGrid.SelectedCells[0].OwningRow.DataBoundItem).UndoChangeValue();
            this.dataGrid.Refresh();
            for (TreeNode node = this.treeView.SelectedNode; node.Parent != null; node = node.Parent)
            {
                this.SetNodeColor(node);
            }
        }
    }
}

