SELECT	sc.*, sca.*, pa.*, pc.*
FROM	Sales.Customer sc
INNER JOIN
		Sales.CustomerAddress sca
	ON	sc.CustomerID = sca.CustomerID
INNER JOIN
		Person.Address pa
	ON	sca.AddressID = pa.AddressID
INNER JOIN
		Person.Contact pc
	ON	sc.CustomerID = pc.ContactID
INNER JOIN
		HumanResources.Shift
	ON	1=1
WHERE	1=1
