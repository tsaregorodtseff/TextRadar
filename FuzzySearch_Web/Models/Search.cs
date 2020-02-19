using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TextRadarFuzzySearchDemo.Models
{
    public struct WordsOfStringTableRow
    {
        public char Simbol;
        public int IndexInString;
        public int NumberOfWord;
        public int SizeOfWord;
        public int NumberInWord;
    }

    public class GroupsTableRow
    {
        public int DiagonalIndex;
        public int InitiallIndexInDiagonal;
        public int FinalDiagonalIndex;
        public int GroupSize;
        public int GroupStartCoordinate;
        public int GroupEndCoordinate;
        public int NumberOfWordOfSearchString;
        public int StartNumberInSearchStringWord;
        public int WordSizeOfSearchString;
        public bool ThisIsInitialGroupOfSearchStringWord;
        public int NumberOfWordOfDataString;
        public int StartNumberInDataStringWord;
        public int WordSizeOfDataString;
        public bool ThisIsInitialGroupOfDataStringWord;
        public int DistanceToParentGroup;
        public bool Disabled;
    }

    public class Search
    {

        public string DataString, SearchString, BriefDisplayOfFoundFragments, FullDisplayOfFoundFragments;
        public int[] BriefDisplayedFragments;

        public double Relevance;

        public int Mode; // 0 - предварительный (быстрый) расчет; 1 - расчет списка результатов; 2 - детальный расчет для отображения страницы; 3 - для поиска в строке данных
        public int Index;

        private int Opt_MinGroupSize;
        private bool Opt_DeleteIntersections;
        private bool Opt_DeleteIntersectionsGroupsInWordsDatatringAndSearchString;
        private bool Opt_ControlConformityAttributeInitialGroupWord;
        private bool Opt_QuickCalculationOfRelevance;
        private bool Opt_FormingFullRepresentationOfResult;
        private bool Opt_FormingBriefRepresentationOfResult;

        private bool ThisIsUselessSymbol(char Simbol)
        {
            return Simbol == '.'
                || Simbol == ','
                || Simbol == ';'
                || Simbol == '('
                || Simbol == ')'
                || Simbol == '['
                || Simbol == ']'
                || Simbol == '/'
                || Simbol == '\\'
                || Simbol == '-'
                || Simbol == '–'
                || Simbol == '*'
                || Simbol == '»'
                || Simbol == '«'
                || Simbol == '\"'
                || Simbol == ':'
                || Simbol == '?'
                || Simbol == '!'
                || Simbol == '…'
                || Simbol == '\'';
        }

        private void FillTableOfDataStringWords(ref string SourceString, ref WordsOfStringTableRow[] TableOfStringWords, int StringBorder)
        {

            int NumberOfWord = 0;
            int NumberInWord = 0;
            bool BodyOfWord = false;
            int CurrentWordRowIndex = -1;
            WordsOfStringTableRow TableRow;
            char SymbolOfRow;
            bool EmptySymbol;
            int IndexOfLastRowOfWord;
            int SizeOfWord;

            for (int i = 0; i <= StringBorder; i++)
            {

                SymbolOfRow = SourceString[i];
                EmptySymbol = SymbolOfRow == ' ' || ThisIsUselessSymbol(SymbolOfRow);

                if (BodyOfWord == false && EmptySymbol == false)
                {
                    BodyOfWord = true;
                    NumberOfWord += 1;
                    NumberInWord = 1;
                }
                else if (BodyOfWord == false && EmptySymbol == true)
                {

                }
                else if (BodyOfWord == true && EmptySymbol == false)
                {
                    NumberInWord += 1;
                }
                else if (BodyOfWord == true && EmptySymbol == true)
                {
                    IndexOfLastRowOfWord = CurrentWordRowIndex;
                    SizeOfWord = TableOfStringWords[CurrentWordRowIndex].NumberInWord;
                    for (int j = 0; j <= SizeOfWord - 1; j++)
                    {
                        TableOfStringWords[IndexOfLastRowOfWord - j].SizeOfWord = SizeOfWord;
                    }
                    BodyOfWord = false;
                    NumberInWord = 0;
                }

                TableRow = new WordsOfStringTableRow();
                TableRow.Simbol = SymbolOfRow;
                TableRow.IndexInString = i;
                TableRow.NumberOfWord = EmptySymbol == true ? 0 : NumberOfWord;
                TableRow.NumberInWord = NumberInWord;
                TableOfStringWords[i] = TableRow;
                CurrentWordRowIndex = i;

            }

            if (BodyOfWord == true)
            {
                IndexOfLastRowOfWord = CurrentWordRowIndex;
                SizeOfWord = TableOfStringWords[CurrentWordRowIndex].NumberInWord;
                for (int j = 0; j <= SizeOfWord - 1; j++)
                {
                    TableOfStringWords[IndexOfLastRowOfWord - j].SizeOfWord = SizeOfWord;
                }
            }

        }
        private void RemoveIntersections(ref List<GroupsTableRow> GroupsTable, ref GroupsTableRow BestGroup)
        {

            List<GroupsTableRow> DeletedRows = new List<GroupsTableRow>();

            int StartBestGroup, EndBestGroup, StartGroup, EndGroup, Difference;

            for (int i = 0; i < GroupsTable.Count; i++)
            {

                if (GroupsTable[i] == BestGroup)
                {
                    continue;
                }

                if (BestGroup.NumberOfWordOfSearchString == GroupsTable[i].NumberOfWordOfSearchString)
                {

                    StartBestGroup = BestGroup.StartNumberInSearchStringWord;
                    EndBestGroup = BestGroup.StartNumberInSearchStringWord + BestGroup.GroupSize - 1;
                    StartGroup = GroupsTable[i].StartNumberInSearchStringWord;
                    EndGroup = GroupsTable[i].StartNumberInSearchStringWord + GroupsTable[i].GroupSize - 1;

                    if (StartBestGroup < StartGroup && StartGroup <= EndBestGroup && EndBestGroup <= EndGroup)
                    {

                        Difference = EndBestGroup - StartGroup + 1;

                        var tmp = GroupsTable[i];
                        tmp.InitiallIndexInDiagonal += Difference;
                        tmp.GroupSize -= Difference;
                        tmp.GroupStartCoordinate += Difference;
                        tmp.StartNumberInSearchStringWord += Difference;
                        tmp.StartNumberInDataStringWord += Difference;

                        GroupsTable[i] = tmp;

                        if (tmp.GroupSize == 0)
                        {
                            DeletedRows.Add(GroupsTable[i]);
                        }

                    }

                    else if (StartGroup < StartBestGroup && StartBestGroup <= EndGroup && EndGroup <= EndBestGroup)
                    {

                        Difference = EndGroup - StartBestGroup + 1;

                        var tmp = GroupsTable[i];
                        tmp.FinalDiagonalIndex -= Difference;
                        tmp.GroupSize -= Difference;
                        tmp.GroupEndCoordinate -= Difference;

                        GroupsTable[i] = tmp;

                        if (tmp.GroupSize == 0)
                        {
                            DeletedRows.Add(GroupsTable[i]);
                        }

                    }

                    else if (StartGroup >= StartBestGroup && EndGroup <= EndBestGroup)
                    {

                        DeletedRows.Add(GroupsTable[i]);

                    }

                }

            }

            for (int j = 0; j < DeletedRows.Count; j++)
            {

                GroupsTable.Remove(DeletedRows[j]);
            }

            DeletedRows.Clear();

            for (int i = 0; i < GroupsTable.Count; i++)
            {

                if (GroupsTable[i] == BestGroup)
                {
                    continue;
                }

                if (BestGroup.NumberOfWordOfDataString == GroupsTable[i].NumberOfWordOfDataString)
                {

                    StartBestGroup = BestGroup.StartNumberInDataStringWord;
                    EndBestGroup = BestGroup.StartNumberInDataStringWord + BestGroup.GroupSize - 1;
                    StartGroup = GroupsTable[i].StartNumberInDataStringWord;
                    EndGroup = GroupsTable[i].StartNumberInDataStringWord + GroupsTable[i].GroupSize - 1;

                    if (StartBestGroup < StartGroup && StartGroup <= EndBestGroup && EndBestGroup <= EndGroup)
                    {

                        Difference = EndBestGroup - StartGroup + 1;

                        var tmp = GroupsTable[i];
                        tmp.InitiallIndexInDiagonal += Difference;
                        tmp.GroupSize -= Difference;
                        tmp.GroupStartCoordinate += Difference;
                        tmp.StartNumberInSearchStringWord += Difference;
                        tmp.StartNumberInDataStringWord += Difference;

                        GroupsTable[i] = tmp;

                        if(tmp.GroupSize == 0)
                        {
                            DeletedRows.Add(GroupsTable[i]);
                        }

                    }

                    else if(StartGroup < StartBestGroup && StartBestGroup <= EndGroup && EndGroup <= EndBestGroup)
                    {

                        Difference = EndGroup - StartBestGroup + 1;

                        var tmp = GroupsTable[i];
                        tmp.FinalDiagonalIndex -= Difference;
                        tmp.GroupSize -= Difference;
                        tmp.GroupEndCoordinate -= Difference;

                        GroupsTable[i] = tmp;

                        if (tmp.GroupSize == 0)
                        {
                            DeletedRows.Add(GroupsTable[i]);
                        }
     
                    }
                    else if (StartGroup >= StartBestGroup && EndGroup <= EndBestGroup)
                    {

                        DeletedRows.Add(GroupsTable[i]);

                    }

                }

            }

            for (int j = 0; j < DeletedRows.Count; j++)
            {
                GroupsTable.Remove(DeletedRows[j]);
            }
        }

        private void RemoveIntersectionsOfGroupsBySearchStringAndDataStringWords(ref List<GroupsTableRow> GroupsTable, ref GroupsTableRow BestGroup)
        {

            List<GroupsTableRow> DeletedRows = new List<GroupsTableRow>();

            for (int i = 0; i < GroupsTable.Count; i++)
            {

                if (GroupsTable[i] == BestGroup)
                {
                    continue;
                }

                if (GroupsTable[i].NumberOfWordOfSearchString == BestGroup.NumberOfWordOfSearchString
                        && GroupsTable[i].NumberOfWordOfDataString != BestGroup.NumberOfWordOfDataString)
                {
                    DeletedRows.Add(GroupsTable[i]);
                }

                else if (GroupsTable[i].NumberOfWordOfDataString == BestGroup.NumberOfWordOfDataString
                        && GroupsTable[i].NumberOfWordOfSearchString != BestGroup.NumberOfWordOfSearchString)
                {
                    DeletedRows.Add(GroupsTable[i]);
                }

            }

            for (int j = 0; j < DeletedRows.Count; j++)
            {
                GroupsTable.Remove(DeletedRows[j]);
            }

        }

        private void FormChainOfGroups(
            ref List<GroupsTableRow> GroupsTable, ref List<GroupsTableRow> ResultGroups, int DataStringLength, int DepthOfRecursion = 0, GroupsTableRow PreviousGroup = null)
        {

            GroupsTableRow BestGroup = new GroupsTableRow();

            if (GroupsTable.Count > 0)
            {
                    GroupsTable = GroupsTable
                        .OrderByDescending(x => x.GroupSize)
                        .ThenBy(x => x.StartNumberInSearchStringWord)
                        .ThenBy(x => x.StartNumberInDataStringWord)
                        .ThenBy(x => x.DiagonalIndex)
                        .ThenBy(x => x.NumberOfWordOfSearchString)
                        .ToList();

                    BestGroup = GroupsTable[0];
            }
            else
            {
                return;
            }

            ResultGroups.Add(BestGroup);

            if (Opt_DeleteIntersections == true)
            {
                RemoveIntersections(ref GroupsTable, ref BestGroup);
            }


            if (Opt_DeleteIntersectionsGroupsInWordsDatatringAndSearchString)
            {
                RemoveIntersectionsOfGroupsBySearchStringAndDataStringWords(ref GroupsTable, ref BestGroup);
            }

            GroupsTable.Remove(BestGroup);

            FormChainOfGroups(ref GroupsTable, ref ResultGroups, DataStringLength, DepthOfRecursion, BestGroup);

        }

        private int FindSeparator(int InitialIndexOfDataString, char Separator, int NumberOfSeparator, char Direction)
        {
            int IndexOfSeparator = InitialIndexOfDataString;
            int NumberOfFindSeparators = 0;

            if (Direction == 'L')
            {
                IndexOfSeparator = 0;

                for (int i = InitialIndexOfDataString; i >= 0; i--)
                {

                    if (DataString[i] == Separator && NumberOfFindSeparators < NumberOfSeparator)
                    {
                        NumberOfFindSeparators++;
                    }
                    else if (DataString[i] == Separator && NumberOfFindSeparators == NumberOfSeparator)
                    {
                        IndexOfSeparator = i;
                        NumberOfFindSeparators++;
                        break;
                    }

                }
            }

            if (Direction == 'R')
            {
                IndexOfSeparator = DataString.Length - 1;

                for (int i = InitialIndexOfDataString; i < DataString.Length; i++)
                {

                    if (DataString[i] == Separator && NumberOfFindSeparators < NumberOfSeparator)
                    {
                        NumberOfFindSeparators++;
                    }
                    else if (DataString[i] == Separator && NumberOfFindSeparators == NumberOfSeparator)
                    {
                        IndexOfSeparator = i;
                        NumberOfFindSeparators++;
                        break;
                    }

                }
            }

            return IndexOfSeparator;

        }

        private void FillFoundFragments(ref List<GroupsTableRow> ResultGroups,
                ref Stack<int> BriefDisplayCharactersOfDataString,
                ref Stack<int> BriefFoundCharactersInDataString,
                ref Stack<int> BriefFoundCharactersInDataStringExtended)
        {

            for (int i = 0; i < ResultGroups.Count; i++)
            {

                if(ResultGroups[i].Disabled == true)
                {
                    continue;
                }

                int StartOfDisplayRange = FindSeparator(ResultGroups[i].GroupStartCoordinate, ' ', 2, 'L');
                int EndOfDisplayRange = FindSeparator(ResultGroups[i].GroupEndCoordinate, ' ', 2, 'R');

                for (int j = StartOfDisplayRange; j <= EndOfDisplayRange; j++)
                {
                    BriefDisplayCharactersOfDataString.Push(j);
                }

                for (int j = ResultGroups[i].GroupStartCoordinate; j <= ResultGroups[i].GroupEndCoordinate; j++)
                {
                        BriefFoundCharactersInDataString.Push(j);

                }

            }

        }

        private double CalculateCoefficient(
            ref WordsOfStringTableRow[] TableOfStringWords, int SearchStringLength, int DataStringLength, ref List<GroupsTableRow> ResultGroups)
        {

            int GroupsOfResultSum = 0;

            if (Opt_QuickCalculationOfRelevance == true)
            {

                for (int i = 0; i < ResultGroups.Count; i++)
                {
                    if (ResultGroups[i].Disabled == true)
                    {
                        continue;
                    }
                    GroupsOfResultSum += ResultGroups[i].GroupSize * ResultGroups[i].GroupSize;
                }

                return GroupsOfResultSum;

            }

            int SearchStringSum = 0;
            int СurrentNumberOfWord = 0;

            for (int i = 0; i < TableOfStringWords.Length; i++)
            {
                if (TableOfStringWords[i].NumberOfWord != СurrentNumberOfWord && TableOfStringWords[i].NumberOfWord != 0)
                {
                    СurrentNumberOfWord = TableOfStringWords[i].NumberOfWord;
                    SearchStringSum += TableOfStringWords[i].SizeOfWord * TableOfStringWords[i].SizeOfWord;
                }
            }

            int MinCoordinate = DataStringLength - 1;
            int MaxCoordinate = 0;

            for (int i = 0; i < ResultGroups.Count; i++)
            {
                if (ResultGroups[i].Disabled == true)
                {
                    continue;
                }
                if (MinCoordinate > ResultGroups[i].GroupStartCoordinate)
                {
                    MinCoordinate = ResultGroups[i].GroupStartCoordinate;
                }
                if (MaxCoordinate < ResultGroups[i].GroupEndCoordinate)
                {
                    MaxCoordinate = ResultGroups[i].GroupEndCoordinate;
                }
                GroupsOfResultSum += ResultGroups[i].GroupSize * ResultGroups[i].GroupSize;
            }

            int ChainOfGroupsLength = MaxCoordinate - MinCoordinate + 1;
            double CoefficientOfMatchSum = SearchStringSum == 0 ? 0.0 : Math.Sqrt((double)GroupsOfResultSum / SearchStringSum);
            double CoefficientOfSize = CoefficientOfMatchSum == 0 ? 0.0 : ChainOfGroupsLength < SearchStringLength ? (double)ChainOfGroupsLength / SearchStringLength : (double)SearchStringLength / ChainOfGroupsLength;

            return 0.9 * CoefficientOfMatchSum + 0.1 * CoefficientOfSize;

        }


        public int CalculateRelevance()
        {

            if (Mode == 0)
            {

                Opt_MinGroupSize = 2;
                Opt_DeleteIntersections = true;
                Opt_DeleteIntersectionsGroupsInWordsDatatringAndSearchString = true;
                Opt_ControlConformityAttributeInitialGroupWord = true;
                Opt_QuickCalculationOfRelevance = true;
                Opt_FormingFullRepresentationOfResult = false;
                Opt_FormingBriefRepresentationOfResult = false;


            }
            else if (Mode == 1)
            {

                Opt_MinGroupSize = 1;
                Opt_DeleteIntersections = true;
                Opt_DeleteIntersectionsGroupsInWordsDatatringAndSearchString = true;
                Opt_ControlConformityAttributeInitialGroupWord = true;
                Opt_QuickCalculationOfRelevance = false;
                Opt_FormingFullRepresentationOfResult = false;
                Opt_FormingBriefRepresentationOfResult = true;

            }
            else if (Mode == 2)
            {

                Opt_MinGroupSize = 1;
                Opt_DeleteIntersections = true;
                Opt_DeleteIntersectionsGroupsInWordsDatatringAndSearchString = true;
                Opt_ControlConformityAttributeInitialGroupWord = true;
                Opt_QuickCalculationOfRelevance = false;
                Opt_FormingFullRepresentationOfResult = true;
                Opt_FormingBriefRepresentationOfResult = true;

            }
            else if (Mode == 3)
            {

                Opt_MinGroupSize = 1;
                Opt_DeleteIntersections = true;
                Opt_DeleteIntersectionsGroupsInWordsDatatringAndSearchString = true;
                Opt_QuickCalculationOfRelevance = false;
                Opt_FormingFullRepresentationOfResult = true;
                Opt_FormingBriefRepresentationOfResult = true;

            }

            CalculateRelevancePrivate();
            return 0;
        }


        private int CalculateRelevancePrivate()

        {

            if (this.SearchString == null || this.DataString == null)
            {
                Relevance = 0.0;
                return 0;
            }

            string SearchString = this.SearchString;
            string DataString = this.DataString;

            int SearchStringLength = SearchString.Length;
            int DataStringLength = DataString.Length;

            int SearchStringBorder;
            int DataStringBorder;

            SearchString = SearchString.ToLower();
            DataString = DataString.ToLower();

            if (SearchStringLength == 0 || DataStringLength == 0 || SearchStringLength > 200 || DataStringLength > 3000)
            {
                Relevance = 0.0;
                return 0;
            }

            string StringBuffer = "";
            for (int i = 0; i < SearchStringLength; i++)
            {
                var SymbolOfRow = SearchString[i];
                if(SymbolOfRow == ' ' || ThisIsUselessSymbol(SymbolOfRow))
                {
                    SymbolOfRow = ' ';
                }
                StringBuffer += SymbolOfRow;
            }
            SearchString = StringBuffer;

            if (DataStringLength < SearchStringLength)
            {
                StringBuffer = "";
                for (int i = 1; i <= SearchStringLength - DataStringLength; i++)
                    StringBuffer += '♦';
                DataString += StringBuffer;
                DataStringLength = DataString.Length;
            }

            SearchStringBorder = SearchStringLength - 1;
            DataStringBorder = DataStringLength - 1;

            bool[] MatchMatrix = new bool[DataStringLength * SearchStringLength];

            for (int i = 0; i <= DataStringBorder; i++)
            {
                for (int j = 0; j <= SearchStringBorder; j++)
                {
                    if (
                        (SearchString[j] == DataString[i]) && SearchString[j] != ' '
                       )
                    {
                        MatchMatrix[SearchStringLength * i + j] = true;
                    }
                    else
                    {
                        MatchMatrix[SearchStringLength * i + j] = false;
                    }
                }
            }

            int DiagonalBorder;
            int StartIndexDiagonal;

            DiagonalBorder = DataStringBorder + SearchStringBorder;

            bool[] DiagonalsValueMatrix = new bool[(DiagonalBorder + 1) * SearchStringLength];

            for (int i = 0; i <= DiagonalBorder; i++)
            {
                for (int j = 0; j <= SearchStringBorder; j++)
                {
                    StartIndexDiagonal = i - SearchStringBorder + j;
                    if (StartIndexDiagonal >= 0 && StartIndexDiagonal <= DataStringBorder)
                        DiagonalsValueMatrix[SearchStringLength * i + j] = MatchMatrix[SearchStringLength * StartIndexDiagonal + j];
                    else
                        DiagonalsValueMatrix[SearchStringLength * i + j] = false;
                }
            }

            List<GroupsTableRow> ResultGroups = new List<GroupsTableRow>();

            WordsOfStringTableRow[] TableOfDataStringWords = new WordsOfStringTableRow[DataStringLength];
            FillTableOfDataStringWords(ref DataString, ref TableOfDataStringWords, DataStringBorder);

            WordsOfStringTableRow[]  TableOfSearchStringWords = new WordsOfStringTableRow[SearchStringLength];
            FillTableOfDataStringWords(ref SearchString, ref TableOfSearchStringWords, SearchStringBorder);

            List<GroupsTableRow> GroupsTable = new List<GroupsTableRow>();
            GroupsTableRow NewListItem;

            int CurrentGroupSize;

            for (int DiagonalIndex = 0; DiagonalIndex <= DiagonalBorder; DiagonalIndex++)
            {

                CurrentGroupSize = 0;

                for (int PositionInDiagonalIndex = 0; PositionInDiagonalIndex <= SearchStringBorder; PositionInDiagonalIndex++)
                {

                    if (DiagonalsValueMatrix[SearchStringLength * DiagonalIndex + PositionInDiagonalIndex] == true)

                    {
                        CurrentGroupSize += 1;
                    }
                    else
                    {

                        if (CurrentGroupSize >= Opt_MinGroupSize)
                        {
                            var InitiallIndexInDiagonal = PositionInDiagonalIndex - CurrentGroupSize;
                            var WordsTableRowSearchString = TableOfSearchStringWords[InitiallIndexInDiagonal];
                            var ThisIsInitialGroupOfSearchStringWord = WordsTableRowSearchString.NumberInWord == 1;
                            var WordsTableRowDataString = TableOfDataStringWords[DiagonalIndex + InitiallIndexInDiagonal - SearchStringLength + 1];
                            var ThisIsInitialGroupOfDataStringWord = WordsTableRowDataString.NumberInWord == 1;

                            if (Opt_ControlConformityAttributeInitialGroupWord == false || 
                                ThisIsInitialGroupOfSearchStringWord == ThisIsInitialGroupOfDataStringWord)
                            {

                                NewListItem = new GroupsTableRow();
                                NewListItem.DiagonalIndex = DiagonalIndex;
                                NewListItem.InitiallIndexInDiagonal = InitiallIndexInDiagonal;
                                NewListItem.FinalDiagonalIndex = PositionInDiagonalIndex - 1;
                                NewListItem.GroupSize = CurrentGroupSize;
                                NewListItem.GroupStartCoordinate = NewListItem.DiagonalIndex + NewListItem.InitiallIndexInDiagonal - SearchStringLength + 1;
                                NewListItem.GroupEndCoordinate = NewListItem.GroupStartCoordinate + NewListItem.GroupSize - 1;
                                NewListItem.NumberOfWordOfSearchString = WordsTableRowSearchString.NumberOfWord;
                                NewListItem.StartNumberInSearchStringWord = WordsTableRowSearchString.NumberInWord;
                                NewListItem.WordSizeOfSearchString = WordsTableRowSearchString.SizeOfWord;
                                NewListItem.ThisIsInitialGroupOfSearchStringWord = ThisIsInitialGroupOfSearchStringWord;
                                NewListItem.NumberOfWordOfDataString = WordsTableRowDataString.NumberOfWord;
                                NewListItem.StartNumberInDataStringWord = WordsTableRowDataString.NumberInWord;
                                NewListItem.WordSizeOfDataString = WordsTableRowDataString.SizeOfWord;
                                NewListItem.ThisIsInitialGroupOfDataStringWord = ThisIsInitialGroupOfDataStringWord;

                                GroupsTable.Add(NewListItem);

                            }

                            CurrentGroupSize = 0;

                        }
                        else
                        {
                            CurrentGroupSize = 0;
                        }

                    }

                }

                if (CurrentGroupSize >= Opt_MinGroupSize)
                {

                    var InitiallIndexInDiagonal = SearchStringLength - CurrentGroupSize;
                    var WordsTableRowSearchString = TableOfSearchStringWords[InitiallIndexInDiagonal];
                    var ThisIsInitialGroupOfSearchStringWord = WordsTableRowSearchString.NumberInWord == 1;
                    var WordsTableRowDataString = TableOfDataStringWords[DiagonalIndex + InitiallIndexInDiagonal - SearchStringLength + 1];
                    var ThisIsInitialGroupOfDataStringWord = WordsTableRowDataString.NumberInWord == 1;

                    if (Opt_ControlConformityAttributeInitialGroupWord == false || 
                        ThisIsInitialGroupOfSearchStringWord == ThisIsInitialGroupOfDataStringWord)
                    {

                        NewListItem = new GroupsTableRow();
                        NewListItem.DiagonalIndex = DiagonalIndex;
                        NewListItem.InitiallIndexInDiagonal = InitiallIndexInDiagonal;
                        NewListItem.FinalDiagonalIndex = SearchStringLength - 1;
                        NewListItem.GroupSize = CurrentGroupSize;
                        NewListItem.GroupStartCoordinate = NewListItem.DiagonalIndex + NewListItem.InitiallIndexInDiagonal - SearchStringLength + 1;
                        NewListItem.GroupEndCoordinate = NewListItem.GroupStartCoordinate + NewListItem.GroupSize - 1;
                        NewListItem.NumberOfWordOfSearchString = WordsTableRowSearchString.NumberOfWord;
                        NewListItem.StartNumberInSearchStringWord = WordsTableRowSearchString.NumberInWord;
                        NewListItem.WordSizeOfSearchString = WordsTableRowSearchString.SizeOfWord;
                        NewListItem.ThisIsInitialGroupOfSearchStringWord = ThisIsInitialGroupOfSearchStringWord;
                        NewListItem.NumberOfWordOfDataString = WordsTableRowDataString.NumberOfWord;
                        NewListItem.StartNumberInDataStringWord = WordsTableRowDataString.NumberInWord;
                        NewListItem.WordSizeOfDataString = WordsTableRowDataString.SizeOfWord;
                        NewListItem.ThisIsInitialGroupOfDataStringWord = ThisIsInitialGroupOfDataStringWord;

                        GroupsTable.Add(NewListItem);

                    }
                }

            }

            FormChainOfGroups(ref GroupsTable, ref ResultGroups, DataStringLength);

            Relevance = CalculateCoefficient(ref TableOfSearchStringWords, SearchStringLength, DataStringLength, ref ResultGroups);

            if (Opt_FormingBriefRepresentationOfResult == true)
                {

                Stack<int> BriefDisplayCharactersOfDataString = new Stack<int>();
                Stack<int> BriefFoundCharactersInDataString = new Stack<int>();
                Stack<int> BriefFoundCharactersInDataStringExtended = new Stack<int>();

                FillFoundFragments(
                    ref ResultGroups,
                    ref BriefDisplayCharactersOfDataString,
                    ref BriefFoundCharactersInDataString,
                    ref BriefFoundCharactersInDataStringExtended);

                BriefDisplayedFragments = BriefDisplayCharactersOfDataString.ToArray<int>();
                Array.Sort<int>(BriefDisplayedFragments);

                var hashset = new HashSet<int>();

                foreach (var x in BriefDisplayedFragments)
                    if (!hashset.Contains(x))
                        hashset.Add(x);

                Array.Resize(ref BriefDisplayedFragments, hashset.Count);
                BriefDisplayedFragments = hashset.ToArray();

                BriefDisplayOfFoundFragments = "";
                int BriefDisplayedFragmentsLength = BriefDisplayedFragments.Length;

                int OriginalDataStringBorder = this.DataString.Length - 1;

                if (BriefDisplayedFragmentsLength > 0)
                {
                    if (BriefDisplayedFragments[0] != 0)
                        BriefDisplayOfFoundFragments += "...";

                    for (int i = 0; i < BriefDisplayedFragmentsLength; i++)
                    {

                        if (BriefDisplayedFragments[i] > OriginalDataStringBorder)
                        {
                            continue;
                        }

                        if (BriefFoundCharactersInDataString.Contains<int>(BriefDisplayedFragments[i]))
                        {
                            BriefDisplayOfFoundFragments += "<span class=\"findefragments\">" + this.DataString[BriefDisplayedFragments[i]] + "</span>";
                        }
                        else
                        {
                            BriefDisplayOfFoundFragments += this.DataString[BriefDisplayedFragments[i]];
                        }

                        if (i < BriefDisplayedFragmentsLength - 1 && BriefDisplayedFragments[i] + 1 != BriefDisplayedFragments[i + 1])
                            BriefDisplayOfFoundFragments += "...";
                    }

                    if (BriefDisplayedFragments[BriefDisplayedFragmentsLength - 1] < OriginalDataStringBorder)
                        BriefDisplayOfFoundFragments += "...";
                }

                if (Opt_FormingFullRepresentationOfResult == true)
                {
                    FullDisplayOfFoundFragments = "";

                    for (int i = 0; i <= OriginalDataStringBorder; i++)
                    {
                        if (BriefFoundCharactersInDataString.Contains<int>(i))
                        {
                            {
                                FullDisplayOfFoundFragments += "<span class=\"select1\">" + this.DataString[i] + "</span>";
                            }
                        }
                        else if (BriefFoundCharactersInDataStringExtended.Contains<int>(i))
                        {
                            FullDisplayOfFoundFragments += "<span class=\"select2\">" + this.DataString[i] + "</span>";
                        }
                        else
                        {
                            FullDisplayOfFoundFragments += this.DataString[i];
                        }
                    }
                }
            }

            return 0;

        }

    }

}