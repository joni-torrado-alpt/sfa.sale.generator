--SELECT [_Field], [_Value] FROM StorageValues sv WHERE _Section = 'OTT' AND _Field LIKE '%Adm%'
SELECT [_Field], [_Value]
FROM (
    SELECT 'OTT_AdmConta_BI'		_Field, '123456789' 			_Value UNION ALL
    SELECT 'OTT_AdmConta_Email'		_Field, 'KAUT-UI5@altice.pt' 	_Value UNION ALL
    SELECT 'OTT_AdmConta_NIF'		_Field, '250657295' 			_Value UNION ALL
    SELECT 'OTT_AdmConta_Nome'		_Field, 'KAUT UG6 KAUT MO3' 	_Value UNION ALL
    SELECT 'OTT_AdmConta_Telefone'  _Field, '213456789' 			_Value UNION ALL
    SELECT 'OTT_AdmConta_Telemovel' _Field, '961234567' 			_Value 
) t