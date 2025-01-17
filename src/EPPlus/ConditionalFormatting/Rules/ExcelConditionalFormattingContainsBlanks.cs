﻿/*************************************************************************************************
  Required Notice: Copyright (C) EPPlus Software AB. 
  This software is licensed under PolyForm Noncommercial License 1.0.0 
  and may only be used for noncommercial purposes 
  https://polyformproject.org/licenses/noncommercial/1.0.0/

  A commercial license to use this software can be purchased at https://epplussoftware.com
 *************************************************************************************************
  Date               Author                       Change
 *************************************************************************************************
  01/27/2020         EPPlus Software AB       Initial release EPPlus 5
  07/07/2023         EPPlus Software AB       Epplus 7
 *************************************************************************************************/
using OfficeOpenXml.ConditionalFormatting.Contracts;
using System.Xml;

namespace OfficeOpenXml.ConditionalFormatting
{
    internal class ExcelConditionalFormattingContainsBlanks : ExcelConditionalFormattingRule,
    IExcelConditionalFormattingContainsBlanks
    {
        /****************************************************************************************/

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="priority"></param>
        /// <param name="worksheet"></param>
        internal ExcelConditionalFormattingContainsBlanks(
          ExcelAddress address,
          int priority,
          ExcelWorksheet worksheet)
          : base(
                eExcelConditionalFormattingRuleType.ContainsBlanks,
                address,
                priority,
                worksheet
                )
        {
            Formula = string.Format(
              "LEN(TRIM({0}))=0",
              Address.Start.Address);
        }

        internal ExcelConditionalFormattingContainsBlanks(ExcelConditionalFormattingContainsBlanks copy, ExcelWorksheet newWs = null) : base(copy, newWs)
        {
        }

        internal override ExcelConditionalFormattingRule Clone(ExcelWorksheet newWs = null)
        {
            return new ExcelConditionalFormattingContainsBlanks(this, newWs);
        }


        void UpdateFormula()
        {
            if(Address != null)
            {
                Formula = string.Format(
                  "LEN(TRIM({0}))=0",
                  Address.Start.Address);
            }
            else
            {
                Formula = string.Format(
                  "LEN(TRIM({0}))=0",
                  "#REF!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="worksheet"></param>
        /// <param name="xr"></param>
        internal ExcelConditionalFormattingContainsBlanks(
          ExcelAddress address,
          ExcelWorksheet worksheet,
          XmlReader xr)
          : base(
                eExcelConditionalFormattingRuleType.ContainsBlanks,
                address,
                worksheet,
                xr)
        {
        }

        public override ExcelAddress Address 
        { 
            get { return base.Address; } 
            set { base.Address = value; UpdateFormula(); } 
        }


        #endregion Constructors

        /****************************************************************************************/
    }
}
