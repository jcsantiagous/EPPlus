﻿/*************************************************************************************************
  Required Notice: Copyright (C) EPPlus Software AB. 
  This software is licensed under PolyForm Noncommercial License 1.0.0 
  and may only be used for noncommercial purposes 
  https://polyformproject.org/licenses/noncommercial/1.0.0/

  A commercial license to use this software can be purchased at https://epplussoftware.com
 *************************************************************************************************
  Date               Author                       Change
 *************************************************************************************************
  04/16/2021         EPPlus Software AB       EPPlus 5.7
 *************************************************************************************************/
using OfficeOpenXml;
using OfficeOpenXml.Core.CellStore;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using OfficeOpenXml.Packaging;
using OfficeOpenXml.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace OfficeOpenXml.Core.ExternalReferences
{
    public class ExcelExternalReferenceCollection : IEnumerable<ExcelExternalReference>
    {
        List<ExcelExternalReference> _list=new List<ExcelExternalReference>();
        ExcelWorkbook _wb;
        internal ExcelExternalReferenceCollection(ExcelWorkbook wb)
        {
            _wb = wb;
            LoadExternalReferences();
        }
        internal void AddInternal(ExcelExternalReference externalReference)
        {
            _list.Add(externalReference);
        }
        public IEnumerator<ExcelExternalReference> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
        public int Count { get { return _list.Count; } }
        public ExcelExternalReference this[int index]
        {
            get
            {
                return _list[index];
            }
        }
        internal void LoadExternalReferences()
        {
            XmlNodeList nl = _wb.WorkbookXml.SelectNodes("//d:externalReferences/d:externalReference", _wb.NameSpaceManager);
            if (nl != null)
            {
                foreach (XmlElement elem in nl)
                {
                    string rID = elem.GetAttribute("r:id");
                    var rel = _wb.Part.GetRelationship(rID);
                    var part = _wb._package.ZipPackage.GetPart(UriHelper.ResolvePartUri(rel.SourceUri, rel.TargetUri));
                    var xr = new XmlTextReader(part.GetStream());
                    var index = 1;
                    while (xr.Read())
                    {
                        if (xr.NodeType == XmlNodeType.Element)
                        {
                            switch (xr.Name)
                            {
                                case "externalBook":
                                    AddInternal(new ExcelExternalReference(_wb, xr, part, elem));
                                    break;
                                case "ddeLink":
                                case "oleLink":
                                case "extLst":
                                    break; //Unsupported
                                default:    //If we end up here the workbook is invalid.
                                    break;
                            }
                        }
                    }
                    xr.Close();
                }
            }
        }
        /// <summary>
        /// Delete the external link at the zero-based index.
        /// </summary>
        /// <param name="index">The zero-based index</param>
        public void Delete(int index)
        {
            if(index < 0 || index>=_list.Count)
            {
                throw (new ArgumentOutOfRangeException(nameof(index)));
            }
            Delete(_list[index]);
        }
        /// <summary>
        /// Delete the specifik external link
        /// </summary>
        /// <param name="externalReference"></param>
        public void Delete(ExcelExternalReference externalReference)
        {
            var ix = _list.IndexOf(externalReference);
            
            _wb._package.ZipPackage.DeletePart(externalReference.Part.Uri);

            ExternalLinksHandler.BreakFormulaLinks(_wb, ix, true);
            
            var extRefs = externalReference.WorkbookElement.ParentNode;
            extRefs?.RemoveChild(externalReference.WorkbookElement);
            if(extRefs?.ChildNodes.Count==0)
            {
                extRefs.ParentNode?.RemoveChild(extRefs);
            }
            _list.Remove(externalReference);
        }
        /// <summary>
        /// Clear all external links and break any formula links.
        /// </summary>
        public void Clear()
        {
            if (_list.Count == 0) return;
            var extRefs = _list[0].WorkbookElement.ParentNode;

            ExternalLinksHandler.BreakAllFormulaLinks(_wb);
            while (_list.Count>0)
            {
                _wb._package.ZipPackage.DeletePart(_list[0].Part.Uri);
                _list.RemoveAt(0);
            }

            extRefs?.ParentNode?.RemoveChild(extRefs);
        }

        internal int GetExternalReference(string extRef)
        {
            if (string.IsNullOrEmpty(extRef)) return -1;
            if(extRef.Any(c=>char.IsDigit(c)==false))
            {
                if(HasWebProtocol(extRef))
                {
                    for (int ix = 0; ix < _list.Count; ix++)
                    {
                        if (extRef.Equals(_list[ix].ExternalReferenceUri.OriginalString, StringComparison.OrdinalIgnoreCase))
                        {
                            return ix;
                        }
                    }
                    return -1;
                }
                if (extRef.StartsWith("file:///")) extRef = extRef.Substring(8);
                var fi = new FileInfo(extRef);
                int ret=-1;
                for (int ix=0;ix<_list.Count;ix++)
                {
                    var fileName = _list[ix].ExternalReferenceUri.OriginalString;
                    if(HasWebProtocol(_list[ix].ExternalReferenceUri.OriginalString))
                    {
                        if(fileName.Equals(extRef, StringComparison.OrdinalIgnoreCase))
                        {
                            return ix;
                        }
                        continue;
                    }
                    if (fileName.StartsWith("file:///")) fileName = fileName.Substring(8);
                    var erFile = new FileInfo(fileName);
                    if(fi.FullName==erFile.FullName)
                    {
                        return ix;
                    }
                    else if (fi.Name==erFile.Name)
                    {
                        ret = ix; 
                    }
                }
                return ret;
            }
            else
            {
                var ix = int.Parse(extRef)-1;
                if(ix<_list.Count)
                {
                    return ix;
                }
            }
            return -1;
        }

        private static bool HasWebProtocol(string fileName)
        {
            return fileName.StartsWith("http:") || fileName.StartsWith("https:") || fileName.StartsWith("ftp:");
        }
    }
}
