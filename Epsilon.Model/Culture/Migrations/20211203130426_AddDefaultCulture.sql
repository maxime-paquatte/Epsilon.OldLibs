ALTER TABLE Ep.tCulture ADD
	IsDefault bit not null
	constraint DF_tCulture_IsDefault default(0)
