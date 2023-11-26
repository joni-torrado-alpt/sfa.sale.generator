DECLARE @offerId bigint = @paramOfferId
DECLARE @offerName VARCHAR(MAX) = @paramOfferName
DECLARE @compostoId bigint, @componentId bigint
SELECT @compostoId = dpa.DefProdutoCompostoID, @componentId = dpa.DefProdutoComponenteID 
FROM DefProdutoAssociacao dpa 
WHERE 	1 = 1
    AND (
            dpa.DefProdutoCompostoID IN (SELECT DefProdutoID FROM DefProduto dp WHERE TipoDefProdutoID = 4 AND (DefProdutoID = @offerId OR Nome = @offerName))
        OR  dpa.DefProdutoComponenteID IN (SELECT DefProdutoID FROM DefProduto dp WHERE TipoDefProdutoID = 1 AND (DefProdutoID = @offerId OR Nome = @offerName))
        )
    AND EXISTS(SELECT 1 FROM DefProduto dp WHERE dp.DefProdutoID = dpa.DefProdutoCompostoID AND DP.DataLancamento < GETDATE() AND DP.DataDescontinuacao > GETDATE() )
    AND EXISTS(SELECT 1 FROM DefProduto dp WHERE dp.DefProdutoID = dpa.DefProdutoComponenteID AND DP.DataLancamento < GETDATE() AND DP.DataDescontinuacao > GETDATE() )
-----------------------------------------------
SELECT TOP 1 dp.DefProdutoId, dp.Nome, c.DefProdutoCategoriaID, c.DefProdutoCategoriaDescricao, c.Classificacao, c.MacroSegmento, c.CampanhaNome, c.CampanhaAbreviatura, c.DefProdutoID DefProdutoCompostoID
        ,(SELECT STUFF((
                SELECT ',' + ISNULL(pgts.TipoServicoSelec, '')
                FROM dbo.PermissaoGrupoTipoServico pgts 
                WHERE 	pgts.IdProdutoComercial = DPP.ValorDefeito 
                    AND EXISTS(SELECT 1 FROM dbo.DefProdutoPropriedade i WHERE i.DefProdutoId = dp.DefProdutoId AND i.Nome = pgts.Propriedade AND i.ValorDefeito = pgts.Valor) 
                GROUP BY pgts.TipoServicoSelec 
                ORDER BY 1 FOR XML PATH('')
            ), 1, LEN(','), NULL)) PermissaoGrupoServico
            ,c.AgenteVendas
FROM dbo.DefProduto dp (NOLOCK)
    CROSS APPLY (
        SELECT   c.Nome CampanhaNome, c.Abreviatura CampanhaAbreviatura, vpwci.DefProdutoID, dpPromo.DefProdutoCategoriaID, dpPromo.DefProdutoCategoriaDescricao, dpPromo.Classificacao, dpPromo.MacroSegmento
                ,CASE c.TipoOrigemID
	        		WHEN 55 THEN
        					'Sem Canal - Residencial'
        			WHEN 70 THEN
        					'Sic_officebox'
        			ELSE
						(SELECT TOP 1 l.Nome FROM Login L (NOLOCK) INNER JOIN Parceiro P (NOLOCK) ON P.ContaID = L.ContaGestoraID INNER JOIN MGPEC_Parceiros_AgenteVendas MPA (NOLOCK) ON MPA.ContaIDSFA = P.ContaID WHERE MPA.TipoEspecializacao IN ( 'CO', 'PA' ) AND MPA.DataInicioColaboracaoMGPEC < GETDATE() AND MPA.DataFimColaboracaoMGPEC > GETDATE() AND MPA.Estado IN ( 'AC' ) AND L.EstadoID = 1 AND p.TipoOrigemID = 6 AND l.PermiteVenderOutros = 1 ORDER BY l.Login DESC)
        		END AgenteVendas
        FROM dbo.Campanha c (NOLOCK) 
            INNER JOIN dbo.ViewPromocaoWithCampanhaIdx vpwci (NOLOCK) ON vpwci.DataLancamento < GETDATE() AND vpwci.DataDescontinuacao > GETDATE() AND vpwci.CampanhaID = c.CampanhaID
            INNER JOIN dbo.ViewDefProdutos dpPromo (NOLOCK) ON vpwci.DefProdutoID = dpPromo.DefProdutoID --AND c.MacroSegmento = dpPromo.MacroSegmento
        WHERE 	1 = 1
            AND vpwci.DefProdutoID = @compostoId
    ) c
    OUTER APPLY (
        SELECT ValorDefeito FROM dbo.DefProdutoPropriedade dpp (NOLOCK) WHERE dpp.DefProdutoID = dp.DefProdutoID AND dpp.Nome = 'ID_Produto_Comercial'
    ) dpp
WHERE   dp.DefProdutoID = @componentId
    --AND c.CampanhaNome LIKE '%PILOTO%'
ORDER BY CASE WHEN CampanhaNome IN ('PILOTO TESTES', 'PME_PILOTO') THEN 1 ELSE 0 END DESC