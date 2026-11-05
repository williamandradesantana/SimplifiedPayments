namespace SimplifiedPayments.Configurations;

public static class ApiInfo
{
    public const string Name = "Simplified Payments API";
    public const string Version = "v1";

    public const string Description = """
        API de pagamentos simplificada, desenvolvida como solução do desafio técnico de backend do PicPay.

        ## Funcionalidades

        - Cadastro, consulta, atualização e remoção de usuários **comuns** (CPF) e **lojistas** (CNPJ)
        - Transferências entre usuários, com consulta a um serviço autorizador externo e notificação de quem recebe

        ## Regras principais

        - Documento e e-mail são únicos
        - Lojistas só recebem transferências
        - O pagador precisa ter saldo suficiente
        - A transferência é transacional: em caso de falha, o saldo do pagador não é alterado
        - Falha na notificação não desfaz a transferência

        Os erros seguem o formato `ProblemDetails` (RFC 9457).
        """;
}
