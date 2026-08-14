/**
 * Formata um valor numérico decimal para string de moeda em reais (R$ 1.234,56)
 */
export function formatarMoeda(valor) {
  if (valor === null || valor === undefined || isNaN(valor)) {
    return 'R$ 0,00';
  }
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL'
  }).format(valor);
}

/**
 * Converte string digitada com máscara para float decimal numérico (ex: "R$ 1.234,56" -> 1234.56)
 */
export function parseMoeda(valorString) {
  if (!valorString) return 0;
  if (typeof valorString === 'number') return valorString;

  const apenasNumeros = valorString.toString().replace(/\D/g, '');
  if (!apenasNumeros) return 0;

  return parseFloat(apenasNumeros) / 100;
}

/**
 * Evento input para máscara monetária em tempo real
 */
export function aplicarMascaraMoedaInput(event) {
  let valor = event.target.value.replace(/\D/g, '');
  if (!valor) {
    event.target.value = '';
    return 0;
  }
  const numero = parseFloat(valor) / 100;
  event.target.value = formatarMoeda(numero);
  return numero;
}