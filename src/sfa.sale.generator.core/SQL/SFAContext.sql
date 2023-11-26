DECLARE @offerId bigint = 0
--DECLARE @offerName VARCHAR(MAX) = 'UZO_ADAB_U2 TV 10+NET 200+MOVEL 6GB____F24__BO_NP_FIBRA'
--DECLARE @offerName VARCHAR(MAX) = 'UZO_ADAB_U2 TV 10+NET 200____F24__BO_NP_FIBRA'
--DECLARE @offerName VARCHAR(MAX) = 'UZO_ADESAO_U2 TV 10+NET 200+MOVEL 6GB____FNA___NP_'
--DECLARE @offerName VARCHAR(MAX) = 'UZO_MCASA_U2 TV 10+NET 200+MOVEL 6GB____F24__BO_NP_FIBRA'
--DECLARE @offerName VARCHAR(MAX) = 'UZO_NI_U2 TV 10+NET 200+MOVEL 6GB____F24__BO_NP_FIBRA'
--DECLARE @offerName VARCHAR(MAX) = 'UZO_NI_U2 TV 10+NET 200____F24__BO_NP_FIBRA'
--DECLARE @offerName VARCHAR(MAX) = 'UZO_REINST_U2 TV 10+NET 200+MOVEL6G____S/FID__BO_NP_FIBRA'
--DECLARE @offerName VARCHAR(MAX) = 'UZO_REINST_U2 TV 10+NET 200____S/FID__BO_NP_FIBRA'
--DECLARE @offerName VARCHAR(MAX) = 'XGS1B_ADAB-F_M3 10000____FNA__BO_NP_FIBRA'--'ADF1_NI_M3_FIBRA_100'--NULL--
--DECLARE @offerName VARCHAR(MAX) = 'NI_MEO_1P TV_HOTELARIA_NP_FIBRA'
--DECLARE @offerName VARCHAR(MAX) = 'MTF1_ADESAO_MEO_1P TV_HOTELARIA_NP_FIBRA'
--DECLARE @offerName VARCHAR(MAX) = 'MTF2_ALTERACAO_MEO_1P TV_HOTELARIA_NP_FIBRA'
--DECLARE @offerName VARCHAR(MAX) = 'MTF2_DESMONTAGEM_MEO_1P TV_HOTELARIA_NP_FIBRA'
DECLARE @offerName VARCHAR(MAX) = 'MTF2_DESM_SOLUCAO_MULTI_TV_NP_FIBRA'
--DECLARE @offerName VARCHAR(MAX) = 'ADF1_NI_M3_FIBRA_100' --QA
--DECLARE @offerName VARCHAR(MAX) = 'NI_M3 500_NBO-M___F24___P_FIBRA' 
--DECLARE @offerName VARCHAR(MAX) = 'NI_M3 500____F24__PAR/EMBBO_NP_FIBRA' --DEV
--DECLARE @offerName VARCHAR(MAX) = 'IPF_ACRESCIMO_MEO_FIBRA_IP_FIXO'
--DECLARE @offerName VARCHAR(MAX) = 'IPF_ACRESCIMO_MEO_FIBRA_IP_FIXO_FU'
--DECLARE @offerName VARCHAR(MAX) = 'IPF_OTT_ACRESCIMO_MEO_FIBRA_IP_FIXO'
--DECLARE @offerName VARCHAR(MAX) = 'IPF_ACRESCIMO_DESCONTO_MEO_FIBRA_IP_FIXO'
--DECLARE @offerName VARCHAR(MAX) = 'IPF_ACRESCIMO_DESCONTO_MEO_FIBRA_IP_FIXO_FU'
--DECLARE @offerName VARCHAR(MAX) = 'IPF_DESMONTAGEM_MEO_FIBRA_IP_FIXO'
--DECLARE @offerName VARCHAR(MAX) = 'ADESAO_MEO GO_AI___S/FID___NP/P_'
--DECLARE @offerName VARCHAR(MAX) = 'MIG_LR+RDIS_FIBRA_PHASE_OUT'
--DECLARE @offerName VARCHAR(MAX) = 'ACRES TV_M3 500___PUB_F0___NP/P_FIBRA'
--DECLARE @offerName VARCHAR(MAX) = 'XGS1B_ADAB-F_M2 NET 1000+VF TTL____S/FID__BO_NP/P_FIBRA'
--DECLARE @offerName VARCHAR(MAX) = 'XGS1B_ADAB-F_M2 NET 100+VF TTL____S/FID__BO_NP/P_FIBRA'
--DECLARE @offerName VARCHAR(MAX) = 'ADS7_DESMONTAGEM_TOTAL_FIBRA_GCP'
--DECLARE @offerName VARCHAR(MAX) = 'ADF2_AP_MEO_TV_FIBRA_NPRES'
--DECLARE @offerName VARCHAR(MAX) = 'XX_SPEED EDITION_ACRES TV__PUB_F24___NP/P_FIBRA'
DECLARE @compostoId bigint, @componentId bigint
SELECT @compostoId = dpa.DefProdutoCompostoID, @componentId = dpa.DefProdutoComponenteID 
--SELECT * 
FROM DefProdutoAssociacao dpa 
WHERE 	1 = 1
    AND (
            dpa.DefProdutoCompostoID IN (SELECT DefProdutoID FROM DefProduto dp WHERE TipoDefProdutoID = 4 AND (DefProdutoID = @offerId OR Nome = @offerName))
        OR  dpa.DefProdutoComponenteID IN (SELECT DefProdutoID FROM DefProduto dp WHERE TipoDefProdutoID = 1 AND (DefProdutoID = @offerId OR Nome = @offerName))
        )
    AND EXISTS(SELECT 1 FROM DefProduto dp WHERE dp.DefProdutoID = dpa.DefProdutoCompostoID AND DP.DataLancamento < GETDATE() AND DP.DataDescontinuacao > GETDATE() )
    AND EXISTS(SELECT 1 FROM DefProduto dp WHERE dp.DefProdutoID = dpa.DefProdutoComponenteID AND DP.DataLancamento < GETDATE() AND DP.DataDescontinuacao > GETDATE() )
IF OBJECT_ID('tempdb..#tempjt') IS NOT NULL BEGIN DROP TABLE #tempjt END
SELECT * INTO #tempjt
FROM (
	SELECT @componentId DefProdutoId, 1 TipoDefProdutoID UNION ALL
	SELECT @compostoId DefProdutoId, 4 TipoDefProdutoID 
) a
--SELECT * FROM #tempjt
-----------------------------------------------

SELECT  dp.DefProdutoId, dp.Nome, c.DefProdutoCategoriaID, c.DefProdutoCategoriaDescricao, c.Classificacao, c.MacroSegmento, Macro, c.CampanhaNome, c.CampanhaAbreviatura, c.DefProdutoID DefProdutoCompostoID
        ,(SELECT STUFF((
                SELECT ',' + ISNULL(pgts.TipoServicoSelec, '')
                FROM dbo.PermissaoGrupoTipoServico pgts 
                WHERE 	pgts.IdProdutoComercial = DPP.ValorDefeito 
                    AND EXISTS(SELECT 1 FROM dbo.DefProdutoPropriedade i WHERE i.DefProdutoId = dp.DefProdutoId AND i.Nome = pgts.Propriedade AND i.ValorDefeito = pgts.Valor) 
                GROUP BY pgts.TipoServicoSelec 
                ORDER BY 1 FOR XML PATH('')
            ), 1, LEN(','), NULL)) PermissaoGrupoServico
            ,PromocaoCampanhaID,CampanhaCampanhaID
            ,c.MultiChannelAvailabilityInCurrent, c.TipoOrigemID, c.AgenteVendas
FROM dbo.DefProduto dp (NOLOCK)
    OUTER APPLY (
        SELECT vpwci.CampanhaID PromocaoCampanhaID, c.Nome CampanhaNome, c.Abreviatura CampanhaAbreviatura, vpwci.DefProdutoID, dpPromo.DefProdutoCategoriaID, dpPromo.DefProdutoCategoriaDescricao, dpPromo.Classificacao, dpPromo.MacroSegmento, c.MacroSegmento Macro
        		, c.CampanhaID CampanhaCampanhaID, c.TipoOrigemID
        		, c.MultiChannelAvailabilityInCurrent 
        		,CASE c.TipoOrigemID
	        		WHEN 55 THEN
        					'Sem Canal - Residencial'
        			WHEN 70 THEN
        					'Sic_officebox'
        			ELSE
						(SELECT TOP 1 l.Nome FROM Login L (NOLOCK) INNER JOIN Parceiro P (NOLOCK) ON P.ContaID = L.ContaGestoraID INNER JOIN MGPEC_Parceiros_AgenteVendas MPA (NOLOCK) ON MPA.ContaIDSFA = P.ContaID WHERE MPA.TipoEspecializacao IN ( 'CO', 'PA' ) AND MPA.DataInicioColaboracaoMGPEC < GETDATE() AND MPA.DataFimColaboracaoMGPEC > GETDATE() AND MPA.Estado IN ( 'AC' ) AND L.EstadoID = 1 AND p.TipoOrigemID = 6 AND l.PermiteVenderOutros = 1 ORDER BY l.Login DESC)
        		END AgenteVendas
        		,CASE c.MultiChannelAvailabilityInCurrent
	        		WHEN 1 THEN
        				CASE WHEN c.TipoOrigemID = 55 THEN
        					'Sem Canal - Residencial'
        				ELSE
        					'Sic_officebox'
        				END 
        			WHEN 0 THEN
						(SELECT TOP 1 l.Nome FROM Login L (NOLOCK) INNER JOIN Parceiro P (NOLOCK) ON P.ContaID = L.ContaGestoraID INNER JOIN MGPEC_Parceiros_AgenteVendas MPA (NOLOCK) ON MPA.ContaIDSFA = P.ContaID WHERE MPA.TipoEspecializacao IN ( 'CO', 'PA' ) AND MPA.DataInicioColaboracaoMGPEC < GETDATE() AND MPA.DataFimColaboracaoMGPEC > GETDATE() AND MPA.Estado IN ( 'AC' ) AND L.EstadoID = 1 AND p.TipoOrigemID = 6 AND l.PermiteVenderOutros = 1 ORDER BY l.Login DESC)
        			ELSE
        				NULL
        		END AgenteVendasOLD
        FROM dbo.Campanha c (NOLOCK) 
            INNER JOIN dbo.ViewPromocaoWithCampanhaIdx vpwci (NOLOCK) ON vpwci.DataLancamento < GETDATE() AND vpwci.DataDescontinuacao > GETDATE() AND vpwci.CampanhaID = c.CampanhaID
            INNER JOIN dbo.ViewDefProdutos dpPromo (NOLOCK) ON vpwci.DefProdutoID = dpPromo.DefProdutoID --AND c.MacroSegmento = dpPromo.MacroSegmento
        WHERE 	1 = 1
	        AND vpwci.DefProdutoID IN (SELECT DefProdutoId FROM #tempjt)
        ) c
    OUTER APPLY (
        SELECT ValorDefeito FROM dbo.DefProdutoPropriedade dpp (NOLOCK) WHERE dpp.DefProdutoID = dp.DefProdutoID AND dpp.Nome = 'ID_Produto_Comercial'
    ) dpp
WHERE 	1 = 1 	
	AND dp.DefProdutoID = (SELECT TOP 1 DefProdutoId FROM #tempjt WHERE TipoDefProdutoID = 1)
	--AND c.CampanhaNome LIKE '%PILOTO%'
	--AND c.CampanhaNome LIKE '%PME_MULTICANAL%'
ORDER BY CASE WHEN CampanhaNome IN ('PILOTO TESTES', 'PME_PILOTO') THEN 1 ELSE 0 END DESC
;

--SELECT TituloDescritivo ,* FROM DefProdutoPropriedade dpp WHERE DefProdutoID IN (SELECT DefProdutoId FROM DefProduto WHERE Nome = 'ACRES TV_M3 500___PUB_F0___NP/P_FIBRA') ORDER BY 1 ;

DECLARE @offerId As int = (SELECT TOP 1 DefProdutoID FROM #tempjt WHERE TipoDefProdutoID = 4)
--DECLARE @offerId As int = 1090128
SELECT FamiliaProdutoId, ParentId, Nome, fp.Activa
FROM FamiliaProduto fp (NOLOCK)
WHERE 	1 = 1
	AND FamiliaProdutoId IN (
	SELECT a.Item 
	FROM dbo.fnSplit(
		(SELECT TOP 1 HPath 
		FROM (
			SELECT HPath, LEN(HPath) Tam_str 
			FROM FamiliaProdutoIdx (NOLOCK)
			WHERE FamiliaProdutoId in (
					SELECT fp.FamiliaProdutoId 
					FROM FamiliaProduto2DefProduto fp (NOLOCK)
						INNER JOIN FamiliaProduto fp2 (NOLOCK) ON fp.FamiliaProdutoId = fp2.FamiliaProdutoId AND fp2.Activa = 1
					WHERE  1 = 1
						--AND EXISTS(SELECT 1 FROM DefProduto dp (NOLOCK) WHERE dp.DefProdutoID = fp.DefProdutoId AND dp.DefProdutoID = @offerId )
						AND fp.DefprodutoId IN (SELECT DefProdutoID FROM #tempjt)
				)
		) A ORDER BY Tam_str),
		'_') a
	)
UNION ALL
	SELECT ovcpt.Id, -1, ovcpt.Description, NULL 
	FROM OC2_ViabilityCoverageProductType ovcpt 
		INNER JOIN OC2_ViabilityCoverageIDRequest_Computed ovcic ON ovcpt.Id = ViabilityCoverageProductType_Id
		INNER JOIN ViewDefProdutoPropriedadeWithIDPedidoIdx vdppwii ON ovcic.Code = vdppwii.IDPedido  
	WHERE 1 = 1
		AND  vdppwii.DefProdutoID IN (SELECT DefProdutoComponenteId FROM DefProdutoAssociacao dpa WHERE dpa.DefProdutoCompostoId = @offerId)
;

SELECT *--c.CampanhaID, Nome CampanhaNome, Abreviatura CampanhaAbreviatura, vpwci.DefProdutoID
FROM dbo.Campanha c (NOLOCK) 
    --INNER JOIN dbo.ViewPromocaoWithCampanhaIdx vpwci ON vpwci.DataLancamento < GETDATE() AND vpwci.DataDescontinuacao > GETDATE()
WHERE 	1 = 1
	--AND vpwci.CampanhaID = c.CampanhaID 
	--AND c.CampanhaID = 867
	--AND c.CampanhaID = 1802
	--AND Nome = 'PME_SITe_GCP'
	AND Nome LIKE '%OTT%'
	--AND c.MacroSegmento = 'PME'
	--AND vpwci.DefProdutoID IN (SELECT DefProdutoID FROM #tempjt)
	--AND Nome LIKE '%PME_DOI_S2S%'

--SELECT * From dbo.DefProdutoPropriedade dpp WHERE Dpp.DefProdutoID = 1097945;
SELECT * FROM Login WHERE Login LIKE 'jtorrado%';

	
SELECT TOP 1 HPath 
FROM (
	SELECT HPath, LEN(HPath) Tam_str 
	FROM FamiliaProdutoIdx
	WHERE 	1 = 1
		AND FamiliaProdutoId in (
			SELECT fpdp.FamiliaProdutoId 
			FROM FamiliaProduto2DefProduto fpdp
				INNER JOIN FamiliaProduto fp (NOLOCK) ON fpdp.FamiliaProdutoId = fp.FamiliaProdutoId AND fp.Activa = 1
			WHERE DefProdutoId IN (SELECT DefProdutoId FROM #tempjt))
) A ORDER BY Tam_str;


-- DEV - Processos Remover 
SELECT * FROM OC2_ProcessValue opv WHERE Deleted = 0 AND Process_Id IN (70, 599,600, 1168, 1169) AND Value IN ('211111111', '221111111','961111111', '963128849', '961234567', '213456789','500960046', '225005000', '969999999','960000000', '911023976','210000000', '123456789','963084572', '500011443','911111111','960000001','20110018','506120180','910000000') ;
UPDATE OC2_ProcessValue SET Deleted = 1, ModifiedOn = GETDATE(), ModifiedBy = (SELECT LoginId FROM Login WHERE Login = 'jtorrado') WHERE Process_Id IN (70,599, 600, 1168, 1169) AND Value IN ('211111111', '221111111','961111111', '963128849', '961234567', '213456789','500960046', '225005000','969999999','960000000', '911023976','210000000', '123456789','963084572', '500011443','911111111','960000001','20110018','506120180','910000000') AND Deleted = 0;
SELECT * FROM OC2_ProcessValue opv WHERE Process_Id IN (70) AND Value IN ('506120180');
SELECT * FROM OC2_ProcessValue opv WHERE Process_Id IN (599) ;



exec dbo.PRC_Campanha_GetByLoginIdAndCanalId @LoginId=52005, @CanalId=16, @AuthenticatedLoginId=201303, @ScriptType='F'

SELECT fp.* 
FROM FamiliaProduto2DefProduto fpdp 
	INNER JOIN FamiliaProduto fp ON fpdp.FamiliaProdutoId = fp.FamiliaProdutoId AND fp.Activa = 1
	INNER JOIN FamiliaProdutoIdx fpi ON fpdp.FamiliaProdutoId = fpi.FamiliaProdutoId 
WHERE 	DefProdutoId IN (SELECT DefProdutoId FROM #tempjt ); 


SELECT DISTINCT TipoServicoID FROM ProdComercialTipoCliente pctc WHERE ProdutoComercialCodigo = '1299';
SELECT * FROM ProdComercialTipoCliente pctc WHERE ProdutoComercialCodigo = '1299' AND Valor = 'MEOFIBRA_NI_MIG' AND Permicao = 1;
SELECT * FROM PermissaoGrupoTipoServico pgts WHERE IdProdutoComercial = '1299' AND Activo = 1 AND Valor = 'MEOFIBRA_NI_MIG';
SELECT * FROM ProdComercialTipoCliente pctc WHERE ProdutoComercialCodigo = '1299' AND Valor = 'MEOFIBRA_ALT_MULTITV' AND Permicao = 1;
SELECT * FROM PermissaoGrupoTipoServico pgts WHERE IdProdutoComercial = '1299' AND Activo = 1 AND Valor = 'MEOFIBRA_ALT_MULTITV';
SELECT * FROM PermissaoGrupoTipoServico pgts WHERE IdProdutoComercial = '1299' AND Activo = 1 AND Valor = 'MEOFIBRA_DESM_PARCIAL_MULTITV';



SELECT * FROM TipoClassificacao tc;
SELECT * FROM FamiliaProduto2DefProduto fpdp WHERE DefProdutoId = 1090128;
SELECT * FROM FamiliaProduto fp WHERE FamiliaProdutoId = 4173;
SELECT * FROM FamiliaProduto fp WHERE FamiliaProdutoId = 4167; 
SELECT * FROM FamiliaProdutoIdx fpi WHERE FamiliaProdutoId = 4173;
SELECT * FROM ViewFamiliaDefProdutoAlls vfdpa WHERE DefProdutoId = 1090128;
SELECT * FROM ViewFamiliaProduto2DefProdutos vfpdp WHERE DefProdutoId = 1090128;
SELECT * FROM RegraCoberturaViabilidade rcv;
SELECT * FROM OC2_ViabilityCoverageRules_All ovcra 



SELECT  --(SELECT MAX(NumeroRequisicao) FROM ViewItemVenda viv (NOLOCK) WHERE viv.DefProdutoID = dp.DefProdutoID And viv.TipoDefProdutoID = 1 AND ISNULL(NumeroRequisicao, '') <> '' ) LastSale,
         (SELECT ValorDefeito FROM DefProdutoPropriedade dpp (NOLOCK) WHERE dpp.DefProdutoID = dp.DefProdutoID AND dpp.Nome = 'Id_Pedido') ID_Pedido
		,(SELECT ValorDefeito FROM DefProdutoPropriedade dpp (NOLOCK) WHERE dpp.DefProdutoID = dp.DefProdutoID AND dpp.Nome = 'Cenario') Cenario
		,(SELECT ValorDefeito FROM DefProdutoPropriedade dpp (NOLOCK) WHERE dpp.DefProdutoID = dp.DefProdutoID AND dpp.Nome = 'CenarioFixo') CenarioFixo
		,(SELECT ValorDefeito FROM DefProdutoPropriedade dpp (NOLOCK) WHERE dpp.DefProdutoID = dp.DefProdutoID AND dpp.Nome = 'NSOM_OfferName') NSOM_OfferName 
		,(SELECT ValorDefeito FROM DefProdutoPropriedade dpp (NOLOCK) WHERE dpp.DefProdutoID = dp.DefProdutoID AND dpp.Nome = 'NSOM_Action') NSOM_Action
		,(SELECT ValorDefeito FROM DefProdutoPropriedade dpp (NOLOCK) WHERE dpp.DefProdutoID = dp.DefProdutoID AND dpp.Nome = 'NSOM_RequestType') NSOM_RequestType
		,(SELECT ValorDefeito FROM DefProdutoPropriedade dpp (NOLOCK) WHERE dpp.DefProdutoID = dp.DefProdutoID AND dpp.Nome = 'NSOM_Scenary_ALT') "NSOM_Scenary_ALT(SpecificAction)"
		,(SELECT ValorDefeito FROM DefProdutoPropriedade dpp (NOLOCK) WHERE dpp.DefProdutoID = dp.DefProdutoID AND dpp.Nome = 'NSOM_RequestMotiveID') NSOM_RequestMotiveID
		--,(SELECT STUFF((SELECT ',' + dpp.Nome FROM DefProdutoPropriedade dpp (NOLOCK) WHERE dpp.DefProdutoID = dp.DefProdutoID AND dpp.Nome LIKE '%MORADA%' ORDER BY 1 FOR XML PATH('')), 1, LEN(','), NULL)) Moradas
		--,(SELECT STUFF((SELECT ',' + dpp.Processador FROM DefProdutoPropriedade dpp (NOLOCK) WHERE dpp.DefProdutoID = dp.DefProdutoID AND dpp.Processador LIKE '%Agendam%' ORDER BY 1 FOR XML PATH('')), 1, LEN(','), NULL)) Processador
		,*
FROM DefProduto dp (NOLOCK)
WHERE 	1 = 1
	AND dp.DataLancamento < GETDATE() AND dp.DataDescontinuacao > GETDATE() AND Nome NOT LIKE '(Rep.)%'
	AND TipoDefProdutoID = 1
	--AND DefProdutoID IN (663901)
	AND Nome NOT LIKE 'NI%' AND Nome NOT LIKE 'XGS1%'
	--AND EXISTS(SELECT 1 FROM ViewItemVenda viv (NOLOCK) WHERE viv.DefProdutoID = dp.DefProdutoID AND ISNULL(NumeroRequisicao, '') <> '')
	--AND EXISTS(SELECT 1 FROM DefProdutoPropriedade dpp (NOLOCK) WHERE dpp.DefProdutoID = dp.DefProdutoID AND dpp.Nome = 'CenarioFixo' AND dpp.ValorDefeito LIKE '%MULTITV%')
	--AND EXISTS(SELECT 1 FROM DefProdutoPropriedade dpp (NOLOCK) WHERE dpp.DefProdutoID = dp.DefProdutoID AND dpp.TituloDescritivo LIKE '%data de satisfação pretendida%')
	--AND EXISTS(SELECT 1 FROM DefProdutoPropriedade dpp (NOLOCK) WHERE dpp.DefProdutoID = dp.DefProdutoID AND dpp.Nome = 'Cenario' AND dpp.ValorDefeito = 'ADD_DESM_P')
	--AND EXISTS(SELECT 1 FROM DefProdutoPropriedade dpp (NOLOCK) WHERE dpp.DefProdutoID = dp.DefProdutoID AND dpp.Nome = 'ID_Pedido' AND dpp.ValorDefeito <> '' AND dpp.ValorDefeito NOT IN ('XGSPO', 'SAT'))
	--AND EXISTS(SELECT 1 FROM DefProdutoPropriedade dpp (NOLOCK) WHERE dpp.DefProdutoID = dp.DefProdutoID AND (dpp.Nome + dpp.ValorDefeito + dpp.Parametros LIKE '%905128%' OR EXISTS(SELECT 1 FROM ListasValores_Propriedades lvp, ListasValores_Valores lvv WHERE lvp.ListasValoresID = lvv.ListasValoresID AND lvp.DefProdutoPropriedadeID = dpp.DefProdutoPropriedadeID AND EXISTS(SELECT 1 FROM dbo.fnSplit('902880,902883,902892,902894,902896,904854', ',') b WHERE CHARINDEX(b.item, lvv.Valor) > 0))))
	--AND DataCriacao > '2023-05-01'
ORDER BY DataCriacao DESC;


SELECT * FROM OC2_ViabilityCoverageProductType;
SELECT * FROM OC2_ViabilityCoverageIDRequest_Computed ovcic WHERE ViabilityCoverageProductType_Id = 4;
exec dbo.PromocoesIDPedidoPorFamiliaProdutos @ascendentes='-1,4167,4173', @familias='4173', @campanha='698', @origem='70', @alvo='70', @parceiroorigem='9572080', @parceiroalvo='10950266', @date='2023-10-11';

SELECT * FROM ViewDefProdutoPropriedadeWithIDPedidoIdx WHERE DefProdutoID = 1090129;

SELECT * FROM StorageValues sv WHERE _Field LIKE '%Cont%';
SELECT [_Field], [_Value] FROM StorageValues sv WHERE _Section = 'OTT' AND _Field LIKE '%Adm%';

SELECT * FROM ViewLogs WHERE LogId = 1495187277;

exec dbo.CollectionValueByFlow  @flowNumber='1564', @flowValue=2,  @strWhere=' id_pedido = ''PP'' AND NSOM_Action = ''A'' ', @strPropWhere='Propriedade = ''CenarioFixo'' AND PropriedadeValor = ''MEOFIBRA_ALT_MULTITV''', @isAddon=0;
SELECT * FROM Collection WHERE Id_Pedido = 'PP' AND NSOM_Action = 'A' AND F1564 = 1 AND PropriedadeValor = 'MEOFIBRA_ALT_MULTITV';

SELECT * FROM dbo.Publicacao WHERE PublicacaoID IN (235, 236);
SELECT * FROM dbo.PublicacaoVisualizacao WHERE PublicacaoID IN (235, 236);
SELECT * FROM dbo.PublicacaoVisualizacao WHERE PerfilID IN (SELECT PerfilID FROM ViewLogins WHERE Login LIKE 'jtorrado%');
SELECT * FROM ViewLogins WHERE Login LIKE 'jtorrado%';
SELECT * FROM Login WHERE email = 'ricardo-m-roquete@telecom.pt' AND EstadoID = 1;
SELECT * FROM Login WHERE Telemovel = '926909310' AND EstadoID = 0;
SELECT * FROM Login WHERE PerfilID = 319 AND EstadoID = 1;

SELECT * 
FROM DefProdutoPropriedade dpp 
WHERE DefProdutoId IN (SELECT DefProdutoID FROM DefProduto dp WHERE dp.Nome = 'MTF2_ALTERACAO_MEO_1P TV_HOTELARIA_NP_FIBRA') 
ORDER BY Nome;

BEGIN TRAN
UPDATE dpp
SET Processador = 'Portal.ModelProcessor.MultiTVApplyContextPropertyProcessor,Portal.ModelProcessor.MultiTVValidatePropertyProcessor'
FROM DefProdutoPropriedade dpp 
WHERE DefProdutoId IN (SELECT DefProdutoID FROM DefProduto dp WHERE dp.Nome = 'MTF2_ALTERACAO_MEO_1P TV_HOTELARIA_NP_FIBRA')
	AND dpp.Nome = 'CenarioFixo';

BEGIN TRAN
UPDATE dpp
SET Processador = 'Portal.ModelProcessor.PortfolioApplyContextPropertyProcessor'
--SELECT * 
FROM DefProdutoPropriedade dpp 
WHERE DefProdutoId IN (SELECT DefProdutoID FROM DefProduto dp WHERE dp.Nome = 'IPF_ACRESCIMO_DESCONTO_MEO_FIBRA_1P_NET')
	AND dpp.Nome = 'CenarioFixo';

ROLLBACK;
COMMIT;



--MulticanalID	;SourcePartner	;Channel;Partner
--70			;9572080		;		;null
-- NULL = -1
exec dbo.GetMGPEC_Vendedores_Multicanal @Channel=-1, @Partner=-1, @Seller=-1, @MulticanalID=70, @SourcePartner=9572080, @TextoProcura='sic';



SELECT * FROM ViewPromocaoWithCampanhaIdx vpwci WHERE CampanhaID = 698;
SELECT * FROM ViewPromocaoWithCampanhaIdx vpwci WHERE CampanhaID = 740 AND DefProdutoID = 1087663;
SELECT * FROM dbo.Campanha WHERE Nome IN ('RETALHO I', 'PILOTO TESTES', 'PME_PILOTO');
SELECT * FROM dbo.Campanha WHERE TipoOrigemID IN (6,16,55, 70,1010);
SELECT * FROM dbo.Campanha WHERE TipoOrigemID IN (16);
SELECT * FROM ViewTipoObjectoOrigems too WHERE TipoObjectoOrigemID IN (6,16,55, 70,1010);
SELECT * FROM TipoObjectoOrigemVisibilidadeMulticanal WHERE TipoObjectoOrigemID = 16;
SELECT * FROM ViewDefProdutos vdp WHERE DefProdutoID = 363645;


SELECT p.TipoOrigemID ,p.CodAgentePT,l.* 
FROM Login L
	INNER JOIN Parceiro P WITH ( NOLOCK ) ON P.ContaID = L.ContaGestoraID
	LEFT JOIN MGPEC_Parceiros_AgenteVendas MPA WITH ( NOLOCK ) ON MPA.ContaIDSFA = P.ContaID
	INNER JOIN TipoObjectoOrigemVisibilidadeMulticanal TOOVM WITH ( NOLOCK ) ON P.TipoOrigemID = TOOVM.TipoObjectoOrigemID
WHERE 	MPA.TipoEspecializacao IN ( 'CO', 'PA' )
    AND MPA.DataInicioColaboracaoMGPEC < GETDATE()
    AND MPA.DataFimColaboracaoMGPEC > GETDATE()
    AND MPA.Estado IN ( 'AC' )
    AND L.EstadoID = 1
    AND p.TipoOrigemID = 6
    --AND TOOVM.Multicanal = 70--@MulticanalID
    --AND l.Nome IN ('Sic_officebox','BO_WIRELINE', 'Sem Canal - Residencial')
    --AND ISNUMERIC(l.Login) = 1
    AND l.PermiteVenderOutros = 1
    --AND l.Nome LIKE '%Sem%'
ORDER BY l.Login DESC;



UPDATE sfa.sfacontext SET IsCompleted = 0;

SELECT * FROM sfa.sfacontext ORDER BY 1 DESC;
SELECT * FROM sfa.SfaContextClientAddress scca;
SELECT * FROM sfa.SfaContextMasterUser scmu;
SELECT * FROM sfa.SfaContextOffer sco ;
SELECT * FROM sfa.SfaContextOfferFamily scof WHERE SfaContextOfferId = 11;
SELECT * FROM sfa.SfaSale ss;
SELECT * FROM sfa.SfaLogType;
SELECT * FROM sfa.sfaLog ORDER BY 1 DESC;

SELECT sc.Id, sc.CreatedOn , sc.LoginId, sc.ClientIdType, sc.ClientIdValue, sc.Environment, sc.IsCompleted
		, sco.PromoId, sco.SimpleId, sco.Name, sco.Campaign, sco.CampaignPassword, sco.CategoryId, sco.TreeNodeSelection, sco.Category, sco.Classification, sco.MacroSegment, sco.SalesAgent 
		, ss.Url, ss.Guid, ss.Duration, ss.CreatedBy, ss.CreatedOn
FROM sfa.sfacontext sc
	INNER JOIN sfa.SfaContextOffer sco ON sc.OfferId = sco.Id
	LEFT JOIN sfa.SfaSale ss ON sc.Id = ss.SfaContextId
WHERE 	1 = 1
	AND sc.Id = 11
	--AND sc.IsCompleted = 1
	--AND sco.Name = 'ADF2_AP_MEO_TV_FIBRA_NPRES'
	--AND ClientIdValue = '1702744765'
ORDER BY sc.Id DESC, sc.CreatedOn DESC;

SELECT sc.Id, sco.Id, sco.PromoId, sco.SimpleId, sco.Name, scof.FamilyId, scof.Name, sco.Campaign, sco.CampaignPassword, sco.CategoryId, sco.TreeNodeSelection, sco.Category, sco.Classification, sco.MacroSegment, sco.SalesAgent 
FROM sfa.SfaContext sc 
	INNER JOIN sfa.SfaContextOffer sco ON sc.OfferId = sco.Id 
	INNER JOIN sfa.SfaContextOfferFamily scof ON sco.Id = scof.SfaContextOfferId 
WHERE sc.Id = 8;

DECLARE @schema VARCHAR(32) = 'sfa'
DECLARE @TablePrefix VARCHAR(32) = ''
--DECLARE @schema VARCHAR(32) = 'dbo'
--DECLARE @TablePrefix VARCHAR(32) = 'CT_'
--DECLARE @TablePrefix VARCHAR(32) = 'CT_Audit'
--SELECT 'ALTER TABLE ODOC.' + TABLE_NAME + ' DROP CONSTRAINT ' + CONSTRAINT_NAME, * 
--FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS t 
--	--INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
--WHERE	CONSTRAINT_SCHEMA = 'ODOC'
--	AND CONSTRAINT_TYPE = 'FOREIGN KEY'
--	AND TABLE_NAME NOT LIKE	'Generate%'
--	AND TABLE_NAME NOT LIKE	'Template%'
--GO
-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- 3º Gravar nas respetivas tabelas os erros caso existam (tabela temporária contiver dados)
DECLARE @sqlStatementForUpdate VARCHAR(MAX)
DECLARE @affectedData INT = 0
DECLARE cursorDropFK CURSOR FOR
	SELECT 'ALTER TABLE ' + CONSTRAINT_SCHEMA + '.[' + TABLE_NAME + '] DROP CONSTRAINT [' + CONSTRAINT_NAME + ']'
	FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS t 
		--INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
	WHERE	1 = 1
		AND CONSTRAINT_SCHEMA = @schema
		AND CONSTRAINT_TYPE = 'FOREIGN KEY'
		AND TABLE_NAME LIKE	@TablePrefix + '%'
		--AND TABLE_NAME NOT LIKE	'Generate%'
		--AND TABLE_NAME NOT LIKE	'Template%'
OPEN cursorDropFK
FETCH NEXT FROM cursorDropFK INTO @sqlStatementForUpdate
-- Efetuar o update caso exista dados
WHILE @@FETCH_STATUS = 0 BEGIN
	EXEC(@sqlStatementForUpdate);
	SET @affectedData = @affectedData + 1;
	FETCH NEXT FROM cursorDropFK INTO @sqlStatementForUpdate;
END
PRINT 'CONSTRAINSTs DROPPED: ' + CAST(@affectedData AS VARCHAR(32))
CLOSE cursorDropFK
DEALLOCATE cursorDropFK
------------------------------------------
SET @affectedData = 0
DECLARE cursorImportUpdateTables CURSOR FOR
	SELECT 'DROP TABLE ' + TABLE_SCHEMA + '.[' + TABLE_NAME + ']'
	FROM INFORMATION_SCHEMA.TABLES 
	WHERE TABLE_SCHEMA = @schema
		AND TABLE_NAME LIKE	@TablePrefix + '%'
		--AND TABLE_NAME NOT LIKE	'Generate%'
		--AND TABLE_NAME NOT LIKE	'Template%'
OPEN cursorImportUpdateTables
FETCH NEXT FROM cursorImportUpdateTables INTO @sqlStatementForUpdate
------------------------------------------
-- Efetuar o update caso exista dados
WHILE @@FETCH_STATUS = 0 BEGIN
	EXEC(@sqlStatementForUpdate)
	SET @affectedData = @affectedData + 1
	FETCH NEXT FROM cursorImportUpdateTables INTO @sqlStatementForUpdate
END
------------------------------------------
PRINT 'TABLEs DROPPED: ' + CAST(@affectedData AS VARCHAR(32))
------------------------------------------
CLOSE cursorImportUpdateTables
DEALLOCATE cursorImportUpdateTables