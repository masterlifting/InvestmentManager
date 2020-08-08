USE InvestmentManager

--TRUNCATE TABLE [Prices]
--DBCC CHECKIDENT ('Prices', RESEED, 1)

SELECT * FROM Accounts
SELECT * FROM AccountTransactions
SELECT * FROM BuyRecommendations
SELECT * FROM Coefficients
SELECT * FROM Comissions
SELECT * FROM ComissionTypes
SELECT * FROM Companies
SELECT * FROM Currencies

SELECT
	c.[Name]
	, d.DateOperation
	, d.Amount
FROM Dividends as d
	join Isins as i on d.IsinId = i.Id
	join Companies as c on i.CompanyId = c.Id
		where c.[Name] like ('%ÌÐÑÊ%')
		order by d.DateOperation desc

SELECT * FROM ExchangeRates
SELECT * FROM Exchanges
SELECT * FROM Industries
SELECT * FROM Lots

SELECT 
	c.[Name]
	,p.BidDate
	,p.[Value]
FROM Prices as p 
	join Tickers as t on p.TickerId = t.Id 
	join Companies as c on t.CompanyId = c.Id 
		where c.[Name] like ('%Yandex%') 
		order by p.BidDate

SELECT C.Name, DI.Name FROM Isins as DI JOIN Companies as C ON DI.CompanyId = C.Id order by C.Name
SELECT * FROM ProxyAddresses
SELECT * FROM Ratings
SELECT * FROM Reports where CompanyId = 67 order by DateUpdate desc
SELECT * FROM ReportSources where [Value] like ('polymetal%')
SELECT * FROM Sectors
SELECT * FROM SellRecommendations

SELECT st.* 
FROM StockTransactions as st 
	join Tickers as t on st.TickerId = t.Id
	join Companies as c on t.CompanyId = c.Id
		where c.Id = 67

SELECT * FROM Tickers as T join Companies as C on T.CompanyId = C.Id where C.Id = 96
SELECT * FROM TransactionStatuses
