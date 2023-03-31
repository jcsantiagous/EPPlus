﻿using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup.LookupUtils;
using OfficeOpenXml.FormulaParsing.FormulaExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup
{
    internal class VLookupV2 : ExcelFunction
    {
        public override CompileResult Execute(IEnumerable<FunctionArgument> arguments, ParsingContext context)
        {
            ValidateArguments(arguments, 3);
            var searchedValue = arguments.ElementAt(0).Value;
            var arg1 = arguments.ElementAt(1);
            if (arg1.DataType == DataType.ExcelError) return CompileResult.GetErrorResult(((ExcelErrorValue)arg1.Value).Type);
            var lookupRange = arg1.ValueAsRangeInfo;
            var lookupIndex = ArgToInt(arguments, 2);
            var rangeLookup = false;
            if(arguments.Count() > 3)
            {
                rangeLookup = ArgToBool(arguments, 3);
            }
            var index = -1;
            if(!rangeLookup)
            {
                var scanner = new XlookupScanner(searchedValue, lookupRange, LookupSearchMode.StartingAtFirst, LookupMatchMode.ExactMatch, LookupRangeDirection.Vertical);
                index = scanner.FindIndex();
                if (index < 0)
                {
                    return CreateResult(eErrorType.NA);
                }
            }
            else
            {
                index = LookupBinarySearch.BinarySearch(searchedValue, lookupRange, true, new LookupComparer(LookupMatchMode.ExactMatchReturnNextSmaller), LookupRangeDirection.Vertical);
                index = LookupBinarySearch.GetMatchIndex(index, lookupRange, LookupMatchMode.ExactMatchReturnNextSmaller, true);
                if (index < 0)
                {
                    return CreateResult(eErrorType.NA);
                }
            }
            return CompileResultFactory.Create(lookupRange.GetOffset(index, lookupIndex - 1));
        }
    }
}