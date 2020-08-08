USE InvestmentManager

-- ������� ������ � ����� �������
TRUNCATE TABLE [Reports]
DBCC CHECKIDENT ('Reports', RESEED, 1)
--��������� ����������
DECLARE @CompanyCount INT, @Id VARCHAR(10)
SET @CompanyCount = 1
--��������� ������
DECLARE my_cur CURSOR FOR SELECT OC.Id FROM [PortfolioByPestunov_Test].[dbo].Companies as OC
RIGHT JOIN Companies as C
ON OC.Name = C.Name
--��������� ������
OPEN my_cur
FETCH NEXT FROM my_cur INTO @Id
   --���� ������ � ������� ����, �� ������� � ����
   --� �������� ��� �� ��� ���, ���� �� ���������� ������ � �������
   WHILE @@FETCH_STATUS = 0
   BEGIN
        --�� ������ �������� ����� ��������� ���� �������� ��������� � ������� �����������   
        INSERT INTO [Reports]
		(
			DateUpdate
			,DateReport
			,StockVolume
			,Revenue
			,NetProfit
			,GrossProfit
			,CashFlow
			,Assets
			,Turnover
			,ShareCapital
			,Dividends
			,Obligations
			,LongTermDebt
			,IsChecked
			,CompanyId)
		SELECT 
			GETDATE() as DateUpdate
			,CAST(DATEFROMPARTS([Year],IIF([Quarter]=1,3,IIF([Quarter]=2,6,IIF([Quarter]=3,9,IIF([Quarter]=4,12,0)))),28) AS DATETIME2) as DateReport
			,StockInCirculation
			,Revenue
			,NetProfit
			,GrossProfit
			,CashFlow
			,Assets
			,Turnover
			,ShareCapital
			,Dividends
			,Obligations
			,LongTermDebt
			,Confirm
			,@CompanyCount
		FROM [PortfolioByPestunov_Test].[dbo].[Reports]
		WHERE CompanyId = CAST(@Id AS bigint)
		SET @CompanyCount +=1;
        --��������� ��������� ������ �������
        FETCH NEXT FROM my_cur INTO @Id
   END
   --��������� ������
   CLOSE my_cur
   DEALLOCATE my_cur
GO
