USE InvestmentManager

-- перемещаем цены в пустую таблицу
TRUNCATE TABLE [Prices]
DBCC CHECKIDENT ('Prices', RESEED, 1)
DECLARE @CompanyCount INT, @Id VARCHAR(10), @tickerId bigint, @currencyId bigint
SET @CompanyCount = 1
--объявляем курсор
DECLARE my_cur CURSOR FOR SELECT OC.Id FROM [PortfolioByPestunov_Test].[dbo].Companies as OC
RIGHT JOIN Companies as C ON OC.[Name] = C.[Name]
--открываем курсор
OPEN my_cur
FETCH NEXT FROM my_cur INTO @Id
   --если данные в курсоре есть, то заходим в цикл
   --и крутимся там до тех пор, пока не закончатся строки в курсоре
   WHILE @@FETCH_STATUS = 0
   BEGIN
		if CAST(@Id as int) = 10
			begin
				SET @CompanyCount +=1;
				--считываем следующую строку курсора
				FETCH NEXT FROM my_cur INTO @Id
			end

		set @tickerId = CAST(@CompanyCount AS bigint)
		
		if (select top 1 ExchangeId from Tickers where Id = @tickerId) = 1
			set @currencyId = 2
		else
			set @currencyId = 1

        --на каждую итерацию цикла запускаем нашу основную процедуру с нужными параметрами   
        INSERT INTO [Prices]
		(
			DateUpdate
			, BidDate
			, [Value]
			,TickerId
			,CurrencyId)
		SELECT 
			GETDATE() as DateUpdate
			,CP.[Date]
			,CP.Price
			,@tickerId
			,@currencyId
		FROM [PortfolioByPestunov_Test].[dbo].[CompanyPricies] AS CP  WHERE CP.CompanyId = CAST(@Id AS bigint)

		SET @CompanyCount +=1;
        --считываем следующую строку курсора
        FETCH NEXT FROM my_cur INTO @Id
   END
   --закрываем курсор
   CLOSE my_cur
   DEALLOCATE my_cur
GO