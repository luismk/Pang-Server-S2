namespace Pangya_GameServer.PangyaEnums
{
    public enum eChatMsg : byte
    {
        /// <summary>
        /// Chat normal entre jogadores / Sussurros (Usa FilteringHack)
        /// </summary>
        NORMAL = 0x00,

        /// <summary>
        /// Informação de Usuário (Ex: "[Info] Jogador entrou")
        /// </summary>
        USER_INFO = 0x01,

        /// <summary>
        /// Mensagem de Sistema (Branco/Cinza padrão)
        /// </summary>
        SYSTEM = 0x02,

        /// <summary>
        /// Anúncios ou Macros de fala
        /// </summary>
        NOTICE_MACRO = 0x03,

        /// <summary>
        /// Aviso de GM ou Eventos (Destaque Dourado/Amarelo)
        /// </summary>
        GM_EVENT = 0x04,

        /// <summary>
        /// Mensagens de Guilda (Cor Azul/Verde característica)
        /// </summary>
        GUILD = 0x05,

        /// <summary>
        /// Alertas de Erro ou Avisos Críticos (Cor Vermelha)
        /// </summary>
        ERROR_ALERT = 0x06,

        /// <summary>
        /// Aviso Especial (Concatenação direta Nickname + Mensagem)
        /// </summary>
        SPECIAL_NOTICE = 0x07
    }
}
