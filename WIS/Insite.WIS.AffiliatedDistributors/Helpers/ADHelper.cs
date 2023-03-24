using Insite.WIS.AffiliatedDistributors.Constants;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insite.WIS.AffiliatedDistributors.Helpers;

internal static class ADHelper
{
    public static DataSet ConvertColumnNames(DataSet source)
    {
        var columns = source.Tables[0].Columns;
        if (!columns.Contains(ADDataFeedSourceFile.MyPartNumberColumn))
        {
            if (columns.Contains(ADDataFeedSourceFile.MemberPartNumberColumn))
            {
                columns[ADDataFeedSourceFile.MemberPartNumberColumn].ColumnName =
                    ADDataFeedSourceFile.MyPartNumberColumn;
            }
            else if (columns.Contains(ADDataFeedSourceFile.MemberPartNumberColumnSpaceDelimited))
            {
                columns[ADDataFeedSourceFile.MemberPartNumberColumnSpaceDelimited].ColumnName =
                    ADDataFeedSourceFile.MyPartNumberColumn;
            }
        }

        return source;
    }
}
