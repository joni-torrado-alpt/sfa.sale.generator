DECLARE @offerId As int = @paramOfferId
SELECT FamiliaProdutoId, ParentId, Nome
FROM dbo.FamiliaProduto fp (NOLOCK)
WHERE FamiliaProdutoId IN (
            SELECT a.Item
            FROM dbo.fnSplit(
                (SELECT TOP 1 HPath 
                FROM (
                    SELECT HPath, LEN(HPath) Tam_str 
                    FROM dbo.FamiliaProdutoIdx (NOLOCK)
                    WHERE FamiliaProdutoId IN (
                            SELECT fpdp.FamiliaProdutoId 
                            FROM dbo.FamiliaProduto2DefProduto fpdp (NOLOCK) 
                                INNER JOIN FamiliaProduto fp (NOLOCK) ON fpdp.FamiliaProdutoId = fp.FamiliaProdutoId AND fp.Activa = 1
                            WHERE EXISTS(SELECT 1 FROM DefProduto dp (NOLOCK) WHERE dp.DefProdutoID = fpdp.DefProdutoId AND dp.DefProdutoID = @offerId ))
                ) A ORDER BY Tam_str),
                '_') a
        )
UNION ALL
SELECT ovcpt.Id FamiliaProdutoId, -1 ParentId, ovcpt.Description Nome
FROM dbo.OC2_ViabilityCoverageProductType ovcpt (NOLOCK) 
    INNER JOIN dbo.OC2_ViabilityCoverageIDRequest_Computed ovcic (NOLOCK) ON ovcpt.Id = ViabilityCoverageProductType_Id
    INNER JOIN dbo.ViewDefProdutoPropriedadeWithIDPedidoIdx vdppwii (NOLOCK) ON ovcic.Code = vdppwii.IDPedido  
WHERE 1 = 1
    AND  vdppwii.DefProdutoID = (SELECT TOP 1 DefProdutoComponenteId FROM dbo.DefProdutoAssociacao dpa (NOLOCK) WHERE dpa.DefProdutoCompostoId = @offerId ORDER By 1 DESC)
