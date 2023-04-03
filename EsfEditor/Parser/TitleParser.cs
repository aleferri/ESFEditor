namespace EsfEditor.Parser
{
    using EsfEditor.Core.Enums;
    using EsfEditor.Core.EsfObjects;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Runtime.InteropServices;

    /*
    * TitleParser is new for 1.4.4.  For Singlnodes and OneOfManyNodes, this class parses unique node titles to replace CA's general names.
    * I refer to the parsed result as a "Title" as opposed to a "Name," as the name is still preserved and stored by node objects.  "Title" replaces
    * "UserName"; the latter appeared in koras321's code, presumably because he intended to allow users to edit node titles.  I decided not to
     * go that route as that would require saving the name edits and would get complicated when dealing with multiple versions of the same file,
     * e.g. edited and unedited verions of startpos.
    * --Erasmus777
    */

    public class TitleParser
    {

        public string GetTitle(IEsfNode node, out List<IEsfNode> children)
        {
            bool shortTitle = false;
            bool found = false;
            if (node.BeenParsed == false)
                node.Parse();
            //Before 1.4.4, getting children was done in EsfEditorForm.  Since needed in both classes, passed back via "out" parameter.
            children = node.GetChildren();    
            //End the method if PolyNode, since they have no values with which to append the node name.
            if (node.Type == EsfValueType.PolyNode)
            {
                node.Title = node.Name;
                return node.Title;
            }
            //Get the node values.
            List<IEsfValue> val = new List<IEsfValue>(node.GetValues());
            string name = string.Empty;
            if (val != null)
                name = TitlesInValues(val, out found);
            //Before 1.4.4, single parent nodes typically share the same names as their child.
            //To reduce redundancy and increase readability, single parents treated as special case.
            if ((children.Count == 1) && (found == false))
            {
                name = SingleParentTitles(children);
                shortTitle = true;  //Tag as a short title for parsing
            }
            return ParseTitle(node, name);
        }

        private string SingleParentTitles(List<IEsfNode> childList)
        {
            //Single parent titles are borrowed from their child's node name.
            IEsfNode node = childList[0];
            bool found = false;
            if (node.BeenParsed == false)
                node.Parse();
            List<IEsfValue> val = new List<IEsfValue>(node.GetValues());
            string name = string.Empty;
            if (val != null)
                name = TitlesInValues(val, out found);
            node.Title = name;
            return name;
        }

        private string TitlesInValues(List<IEsfValue> vals, out bool found)
        {
            found = false;
            int depth = 0; //Prevent loop from going beyond the first 3 values, since that's where the values we want usually live.
            string c = string.Empty;
            foreach (IEsfValue v in vals)
            {
                //  Test for Unicode, as those are the values added to the Title. 
                if ((v.Type == EsfValueType.UTF16) && (v.Value.ToString().Length > 3))
                {
                    c = v.Value.ToString();
                    found = true; //Tag found in values
                    break;
                }
                depth++;
                if (depth == 3)
                    break;
            }
            return c;
        }

        private string ParseTitle(IEsfNode node, string s)
        {
            StringBuilder sB = new StringBuilder();
            char[] c = s.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == ' ') //White space inidicates garbage string or a bad read
                    break;
                sB.Append(c[i]);
            }
            if (sB.Length < 5)  //Test for garbage
                sB = new StringBuilder(node.Name);
            node.Title = sB.ToString();
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(node.Title);   //Capitalize and assign the parsed title
        }

        //private string ParseTitle(IEsfNode node, string s, bool shortTitle, bool found)
        //{
        //    StringBuilder sB = new StringBuilder();
        //    char[] c = s.ToCharArray();
        //    if (c.Length >= 4)  //Limiting the strings parsed to lengths greater than four eliminates most garbage titles
        //    {
        //        for (int i = 0; i < c.Length; i++)
        //        {
        //            if (c[i] == ' ') //White space inidicates garbage string or a bad read
        //                break;
        //            sB.Append(c[i]);
        //        }
        //        if (sB.Length < 4)  //Test length again after eliminating garbage strings.  If its too short, use the default node name.
        //            sB = new StringBuilder(node.Name);
        //        else if ((shortTitle == false) && (found == true))  //To help readibility, append with the default node name if this isn't a single parent and the title was found in its values,.
        //        {
        //            string st = " | " + node.Name;
        //            sB.Append(st);
        //        }
        //    }
        //    //Get a name for anything missed so far
        //    else
        //        sB = new StringBuilder(node.Name);
        //    node.Title = sB.ToString();
        //    return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(node.Title);   //Capitalize and assign the parsed title
        //}

        ////////////////Special cases for CAI and other nodes is still a WIP////////////////

        //public string CAINodeTitle()
        //{
        //    if (node.Name != "CAI_WORLD_FACTIONS")
        //        node.Title = node.Name;
        //    if (node.Name == "CAI_FACTION")
        //    {
        //        foreach (IEsfValue v in this.val)
        //            if ((v.Type == EsfValueType.UInt) && (v.Value.ToString().Length > 7))
        //            {
        //                IEsfNode p = node.Parent;
        //                p.Title = v.Value.ToString();
        //            }
        //    }
        //    //else
        //    //{
        //    //    foreach (IEsfNode child in this.childList)
        //    //        if (child.Name == "CAI_FACTION")
        //    //        {
        //    //            List<IEsfValue> cVal = child.GetValues();
        //    //            foreach(IEsfValue v in cVal)
        //    //                if ((v.Type == EsfValueType.UInt) && (v.Value.ToString().Length > 7))
        //    //                    node.Title = v.Value.ToString();
        //    //        }
        //            else
        //                node.Title = node.Name;
        //    //}
        //    return node.Title;
        //}



        }
    }

        
  
    


