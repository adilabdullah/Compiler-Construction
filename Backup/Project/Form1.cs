using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Project
{
    public partial class Form1 : Form
    {
        public string Code;
   
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.StreamReader Sr = new System.IO.StreamReader(openFileDialog1.FileName);
                Code = Sr.ReadToEnd();
                Sr.Close();

                richTextBox1.Text = Code;
                DGVError.Rows.Clear();
                DGVToken.Rows.Clear();
                List<string> Tokens = Lexical.Analyze(Code);
                foreach (string Token in Tokens)
                {
                    DGVToken.Rows.Add(Token);
                    if (Token.Contains("InvalidLexeme"))
                    {
                        string Error = Token.Substring(Token.IndexOf(",") + 1);
                        string Line = Error.Substring(Error.IndexOf(",") + 2).Replace(" )", "");
                        string Word = Error.Remove(Error.IndexOf(",") - 1);
                        DGVError.Rows.Add(Line, "Lexical Error", "Invalid Word - " + Word);
                        DGVError.Rows[DGVError.Rows.Count - 1].Cells[0].Style.ForeColor = Color.Crimson;
                        DGVError.Rows[DGVError.Rows.Count - 1].Cells[1].Style.ForeColor = Color.Crimson;
                    }
                }

                if (Tokens.Count == 0)
                    button2.Enabled = false;
                else
                    button2.Enabled = true;

                System.IO.StreamWriter Sw = new System.IO.StreamWriter(@"Token Set " + String.Format("{0:hh-mm-ss-tt}", DateTime.Now) + ".txt");
                foreach (string Token in Tokens)
                    Sw.WriteLine(Token);
                Sw.Close();

                System.Diagnostics.Process.Start("notepad.exe", @"Token Set " + String.Format("{0:hh-mm-ss-tt}", DateTime.Now) + ".txt");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            richTextBox1.Text = @"class C1
{
    func head()
    {
C2 obj = create object(C2);
int a=obj=>af();
obj=>c[1]++;
    }
}
class C2{
int a;
int b;
known int[] c;
known func af() ret int
{
int b ;

ret a + b ;
}
}";
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                Compile();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Compile();
        }

        private void Compile()
        {
            richTextBox2.Visible = false;
            label4.Visible = false;
            label3.Visible = true;
            DGVError.Rows.Clear();
            DGVToken.Rows.Clear();
            SymbolTableDGV.Rows.Clear();
            List<string> Tokens = Lexical.Analyze(richTextBox1.Text);
            foreach (string Token in Tokens)
            {
                DGVToken.Rows.Add(Token);
                if (Token.Contains("InvalidLexeme"))
                {
                    string Error = Token.Substring(Token.IndexOf(",") + 1);
                    string Line = Error.Substring(Error.IndexOf(",") + 2).Replace(" )", "");
                    string Word = Error.Remove(Error.IndexOf(",") - 1);
                    DGVError.Rows.Add(Line, "Lexical Error", "Invalid Word - " + Word);
                    DGVToken.Rows[DGVToken.Rows.Count - 1].Cells[0].Style.ForeColor = Color.Crimson;
                }
            }

            if (DGVError.Rows.Count == 0)
            {
                LexicalLabel.Text = "\u2713 No Lexical Error";
                LexicalLabel.ForeColor = SyntaxLabel.ForeColor = SemanticLabel.ForeColor = Color.MediumSeaGreen;
                SyntaxLabel.Visible = true;

                List<string> SyntaxSemanticErrors = Syntax.Analyze(Tokens);

                if (SyntaxSemanticErrors.Count == 0)
                {
                    richTextBox2.Visible = true;
                    richTextBox2.BackColor = Color.White;
                    label3.Visible = false;
                    label4.Visible = true;
                    SyntaxLabel.Text = "\u2713 No Syntax Error";
                    SemanticLabel.Text = "\u2713 No Semantic Error";
                    SemanticLabel.Visible = true;
                }

                else
                {
                    List<string> SyntaxErrors = SyntaxSemanticErrors.Where(x => x.StartsWith("Syntax")).ToList();

                    if (SyntaxErrors.Count != 0)
                    {
                        SemanticLabel.Visible = false;
                        SyntaxLabel.ForeColor = Color.Crimson;

                        SyntaxLabel.Text = "\u2715 " + SyntaxErrors.Count + " Syntax Errors";
                        foreach (string s in SyntaxErrors)
                            DGVError.Rows.Add(s.Split('$')[1], "Syntax Error", s.Split('$')[2]);
                    }

                    else
                    {
                        SyntaxLabel.Text = "\u2713 No Syntax Error";
                        SemanticLabel.Visible = true;

                        List<string> SemanticErrors = SyntaxSemanticErrors.Where(x => x.StartsWith("Semantic")).ToList();

                        if (SemanticErrors.Count == 0)
                            SemanticLabel.Text = "\u2713 No Semantic Error";

                        else
                        {
                            SemanticLabel.ForeColor = Color.Crimson;
                            SemanticLabel.Text = "\u2715 " + SemanticErrors.Count + " Semantic Errors";
                            foreach (string s in SemanticErrors)
                                DGVError.Rows.Add(s.Split('$')[1], "Semantic Error", s.Split('$')[2]);
                        }
                    }
                }
            }
            else
            {
                LexicalLabel.Text = "\u2715 " + DGVError.Rows.Count + " Lexical Errors";
                LexicalLabel.ForeColor = Color.Crimson;
                SyntaxLabel.Visible = SemanticLabel.Visible = false;
            }

            if (Tokens.Count == 0)
            {
                button2.Enabled = false;
                LexicalLabel.Visible = false;
                SyntaxLabel.Visible = false;
                SemanticLabel.Visible = false;
            }
            else
            {
                button2.Enabled = true;
                LexicalLabel.Visible = true;
            }
            for (int i = 0; i < Semantic.ST.Count; i++)
            {
                SymbolTableDGV.Rows.Add(Semantic.ST[i].Name, "class");
                for (int j = 0; j < Semantic.ST[i].Link.Name.Count; j++)
                {
                    SymbolTableDGV.Rows.Add(Semantic.ST[i].Link.Name[j], Semantic.ST[i].Link.Type[j], Semantic.ST[i].Link.Scope[j], Semantic.ST[i].Link.AM[j]);
                    if (Semantic.ST[i].Link.Link.Count != 0)
                    {
                        for (int k = 0; k < Semantic.ST[i].Link.Link[j].Name.Count; k++)
                        {
                            SymbolTableDGV.Rows.Add(Semantic.ST[i].Link.Link[j].Name[k], Semantic.ST[i].Link.Link[j].Type[k], Semantic.ST[i].Link.Link[j].Scope[k]);
                        }
                    }
                }
            }

            System.IO.StreamReader Sr = new System.IO.StreamReader("IC.txt");
            richTextBox2.Text = Sr.ReadToEnd();
            Sr.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string FileName="TokenSet " + String.Format("{0:hh-mm-ss-tt}", DateTime.Now) + ".txt";
            System.IO.StreamWriter Sw = new System.IO.StreamWriter(FileName);
            for (int i = 0; i < DGVToken.Rows.Count; i++)
            {
                Sw.WriteLine(DGVToken.Rows[i].Cells[0].Value);
            }
            Sw.Close();

            System.Diagnostics.Process.Start("notepad.exe", FileName);
        }

        private void DGVToken_SelectionChanged(object sender, EventArgs e)
        {
            DGVToken.ClearSelection();
        }

        private void DGVError_SelectionChanged(object sender, EventArgs e)
        {
            DGVError.ClearSelection();
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void richTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData.ToString() == "OemOpenBrackets, Shift")
            {
                richTextBox1.SelectedText = "\n    \n}";
                richTextBox1.SelectionStart -=2;
            }
        }

        private void SymbolTableDGV_SelectionChanged(object sender, EventArgs e)
        {
            SymbolTableDGV.ClearSelection();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox1.Visible = !checkBox1.Checked;
        }
    }
}













