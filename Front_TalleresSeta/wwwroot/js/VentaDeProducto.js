
// ============================================================================
// 1. VARIABLES Y CONSTANTES (Ordenadas Alfabéticamente)
// ============================================================================

const btnBuscar = document.getElementById('btnBuscarCliente');
let carritoVenta = [];
const cantUnidadesVender = document.getElementById('validarCantidadVendidos');
const chkConsumidorFinal = document.getElementById('chkConsumidorFinal');
const chkManualTotal = document.getElementById('chkManualTotal');
let ConsecutivoActual = null;
const iconoPreviewImagenProducto = document.getElementById('iconoPreviewImagenProducto');
let ImagenProducto = document.getElementById('ImagenProducto');
let inputPrecioVentaPorUni = document.getElementById('validarPrecioVentaPorUni');
const inputPrecioVentaTotal = document.getElementById('validarPrecioVentaTotal');
const lblCantStockActual = document.getElementById('lblCantStockActual');
const lblNumeroPedido = document.getElementById('lblNumeroPedido');
const limpiarNumero = (val) => parseFloat((val || '').toString().replace(/\D/g, '')) || 0;
let lotesActuales = null;
let lotesSeleccionados = [];
let precioVentaBaseOriginal = 0;
let previewImagenProducto = document.getElementById('previewImagenProducto');
let productoCargadoActual = null;
const selectTipoDoc = document.getElementById('TipoDocumentoId');
let selectTipoDoc_antes = null;
const selectTipoVehiculo = document.getElementById('TipoVehiculoId');
let selectTipoVehiculo_antes = null;
const tallerId = document.getElementById("txtTallerId").value;
const txtCorreo = document.getElementById('txtCorreo');
let txtCorreo_antes = null;
const txtDocumento = document.getElementById('txtClienteDocumento');
let txtDocumento_antes = null;
const txtPrimerApellido = document.getElementById('txtPrimerApellido');
let txtPrimerApellido_antes = null;
const txtPrimerNombre = document.getElementById('txtPrimerNombre');
let txtPrimerNombre_antes = null;
const txtSegundoApellido = document.getElementById('txtSegundoApellido');
let txtSegundoApellido_antes = null;
const txtSegundoNombre = document.getElementById('txtSegundoNombre');
let txtSegundoNombre_antes = null;
const txtTelefono = document.getElementById('txtClienteTelefono');
let txtTelefono_antes = null;
const txtVehiculoKm = document.getElementById('txtVehiculoKm');
let txtVehiculoKm_antes = null;
const txtVehiculoModelo = document.getElementById('txtVehiculoModelo');
let txtVehiculoModelo_antes = null;
const txtVehiculoPlaca = document.getElementById('txtVehiculoPlaca');
let txtVehiculoPlaca_antes = null;
const validarCodigo = document.getElementById("validarCodigo");
const validarDetalle = document.getElementById('validarDetalle');
const visualizaColorProducto = document.getElementById('lblColorProducto');
const visualizaMarcaProducto = document.getElementById('lblMarcaProducto');
const visualizaNombreProducto = document.getElementById('lblNombreProducto');
const visualizaReferenciaProducto = document.getElementById('lblReferenciaProducto');



// ============================================================================
// 2. FUNCIONES ASÍNCRONAS (Async - Ordenadas Alfabéticamente)
// ============================================================================

async function ActualizarGananciaVenta(dataGanancia) {
    try {
        const response = await fetch(`/InventarioGanancias/ActualizarGananciaVenta?filtroId=${tallerId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dataGanancia)
        });

        if (response.ok) {
            const resultado = await response.json();
            return resultado.id || true;
        }
        return null;
    } catch (error) {
        console.error("Error al ejecutar SP de ganancia:", error);
        return null;
    }
}

async function ActualizarLotesVenta(dataLoteProducto) {
    try {
        const response = await fetch(`/InventarioLotes/ActualizarLotesVenta?filtroId=${tallerId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dataLoteProducto)
        });

        if (response.ok) {
            const resultado = await response.json();
            return resultado.id || resultado.loteId || true;
        }
        return null;
    } catch (error) {
        console.error("Error al actualizar lote:", error);
        return null;
    }
}

async function ActualizarStockVenta(dataStockProducto) {
    try {
        const response = await fetch(`/InventarioStocks/ActualizarStockVenta?filtroId=${tallerId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dataStockProducto)
        });

        if (response.ok) {
            const resultado = await response.json();
            return resultado.id || resultado.stockId || true;
        }
        return null;
    } catch (error) {
        console.error("Error al actualizar stock:", error);
        return null;
    }
}

async function AgregarProductoAlCarrito() {

    if (!productoCargadoActual) {
        MostrarAlerta("warning", "Atención", "Primero debe buscar y seleccionar un producto.", 4000);
        return;
    }

    let productoActual = productoCargadoActual;

    const yaExiste = carritoVenta.some(item => String(item.codigoProducto) === String(productoActual.codigoProducto));

    if (yaExiste) {
        MostrarAlerta("warning", "Atención", "Este producto ya fue agregado a la lista de venta.", 4000);
        return;
    }

    const cantidad = parseInt(cantUnidadesVender.value, 10) || 0;
    const precioUnid = limpiarNumero(inputPrecioVentaPorUni.value);
    const total = cantidad * precioUnid;

    if (cantidad <= 0 || precioUnid <= 0) {
        MostrarAlerta("warning", "Atención", "Ingrese una cantidad y precio válidos.", 4000);
        return;
    }

    if (!lotesSeleccionados || lotesSeleccionados.length === 0) {
        MostrarAlerta("warning", "Atención", "Debe seleccionar al menos un lote para este producto.", 4000);
        return;
    }

    const distribucionResult = CalcularDistribucionLotes(cantidad);

    if (distribucionResult.unidadesPendientes > 0) {
        MostrarAlerta("warning", "Atención", `Aún faltan ${distribucionResult.unidadesPendientes} unidades por cubrir. Seleccione más lotes.`, 10000);
        return;
    }

    const lotesEstandarizados = distribucionResult.distribucion
        .filter(item => item.tomadas > 0)
        .map(item => {
            const loteObj = lotesActuales.find(l => String(l.loteId ?? l.loteId ?? l.id) === String(item.loteId));
            const id = item.loteId;

            let refLote = loteObj?.referencia || loteObj?.Referencia || loteObj?.nombreLote || loteObj?.codigoLote || loteObj?.numLote;

            if (!refLote || refLote.toString().trim().toUpperCase() === 'NO APLICA' || refLote.toString().trim() === '') {
                refLote = `Lote ${id}`;
            }

            return {
                loteId: id,
                referencia: refLote,
                cantidad: item.tomadas
            };
        });

    let nomProducto = (productoActual.nombreProducto || '').toUpperCase().trim();
    let marcaProducto = (productoActual.nombreMarca || '').toUpperCase().trim();
    let colorProducto = (productoActual.nombreColor || '').toUpperCase().trim();
    let referenciaProducto = (productoActual.referencia || '').toUpperCase().trim();

    let detalleProducto = [nomProducto, marcaProducto, colorProducto, referenciaProducto]
        .filter(val => val !== '' && val !== 'NO APLICA')
        .join(' - ');

    const detalleItem = {
        codigoProducto: productoActual.codigoProducto,
        nombreProducto: detalleProducto,
        cantidad: cantidad,
        precioUnitario: precioUnid,
        total: total,
        lotes: lotesEstandarizados
    };

    // 1. Obtener/Crear Consecutivo de Pedido
    const idPedido = await CargarConsecutivoPedido();
    if (!idPedido) {
        MostrarAlerta("warning", "Error de Pedido", "No se pudo obtener ni generar el consecutivo del pedido.", 5000);
        return;
    }

    // 2. Solo agregar al carrito en memoria (sin afectar BD todavía)
    carritoVenta.push(detalleItem);
    RenderizarTablaVenta();
    LimpiarCamposVenta();
    if (validarCodigo) validarCodigo.value = '';

    if (typeof calcularTotalPagado_Y_Restante === 'function') {
        calcularTotalPagado_Y_Restante(null, null, null);
    }
}

async function AgregarProductoTabla(dataProducto) {
    try {
        const idTaller = parseInt(document.getElementById("txtTallerId")?.value, 10);

        if (!idTaller || isNaN(idTaller)) {
            console.error("El TallerId no es válido.");
            return null;
        }

        const response = await fetch(`/InventarioSalidaProductos/AgregarProductoVenta?filtroId=${idTaller}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify(dataProducto)
        });

        if (response.ok) {
            const resultado = await response.json();
            return resultado.id || resultado.productoId || true;
        } else {
            const errorMsg = await response.text();
            console.error(`Error HTTP ${response.status} en AgregarProductoVenta:`, errorMsg);
            return null;
        }
    } catch (error) {
        console.error("Error al registrar producto en tabla:", error);
        return null;
    }
}

async function BuscarCliente() {
    let clienteEncontrado = false;

    // 1. Aseguramos conversión a String y eliminamos espacios en blanco
    const numeroDocumento = String(txtDocumento?.value || '').trim();

    // 2. Validamos la longitud real del texto
    if (!numeroDocumento || numeroDocumento.length < 6) {
        MostrarAlerta("warning", "Documento incompleto", "Agregue un número de documento válido.", 10000);
        return;
    }

    try {
        const response = await fetch(`/Usuarios/ObtenerUsuario?filtroId=${encodeURIComponent(numeroDocumento)}`);

        if (response.ok) {
            const clienteActual = await response.json();

            // 3. Manejo seguro para determinar si viene un Objeto o un Array con datos
            const esValido = clienteActual && (
                Array.isArray(clienteActual)
                    ? clienteActual.length > 0
                    : Object.keys(clienteActual).length > 0
            );

            if (esValido) {
                // Si la respuesta es un Array, se toma el primer registro
                const cliente = Array.isArray(clienteActual) ? clienteActual[0] : clienteActual;

                clienteEncontrado = true;

                // Mapeo con soporte para camelCase y PascalCase
                if (selectTipoDoc) selectTipoDoc.value = cliente.tipoDocumentos.tipoDocumentoId ?? cliente.tipoDocumentos.tipoDocumentoId ?? '';
                if (txtDocumento) txtDocumento.value = cliente.documento ?? cliente.documento ?? '';
                if (txtPrimerNombre) txtPrimerNombre.value = cliente.primerNombre ?? cliente.primerNombre ?? '';
                if (txtSegundoNombre) txtSegundoNombre.value = cliente.segundoNombre ?? cliente.segundoNombre ?? '';
                if (txtPrimerApellido) txtPrimerApellido.value = cliente.primerApellido ?? cliente.primerApellido ?? '';
                if (txtSegundoApellido) txtSegundoApellido.value = cliente.segundoApellido ?? cliente.segundoApellido ?? '';
                if (txtTelefono) txtTelefono.value = cliente.telefonoMovil ?? cliente.telefonoMovil ?? '';
                if (txtCorreo) txtCorreo.value = cliente.correo ?? cliente.correo ?? '';
            } else {
                MostrarAlerta("warning", "Cliente", "Datos vacíos del usuario.", 10000);
            }
        } else {
            MostrarAlerta("warning", "Cliente", "Usuario no encontrado.", 10000);
        }
    } catch (error) {
        console.error('Error al buscar cliente:', error);
        MostrarAlerta("warning", "Cliente", "Error al buscar usuario: " + error, 10000);
    }
}

async function CargarConsecutivoPedido() {
    if (!tallerId || tallerId <= 0) {
        console.warn("El ID del taller no es válido.");
        return null;
    }

    // Si ya existe en memoria, se retorna inmediatamente
    if (ConsecutivoActual) {
        return ConsecutivoActual;
    }

    try {
        const response = await fetch(`/Pedidos/CrearPedido?filtroId=${tallerId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            const pedido = await response.json();

            if (pedido && pedido.consecutivoPedidoCreado) {
                ConsecutivoActual = pedido.consecutivoPedidoCreado;

                if (lblNumeroPedido) {
                    lblNumeroPedido.textContent = ConsecutivoActual;
                }
                return ConsecutivoActual;
            }
        } else {
            console.error("Error en la respuesta del servidor:", response.statusText);
        }
    } catch (error) {
        console.error("Error al conectar con la API de Pedidos:", error);
    }
    return null;
}

async function CargarLotesProductoVenta(producto) {
    if (!producto || !producto.codigoProducto) {
        console.warn('El producto ingresado no contiene un código válido.');
        return;
    }

    lotesSeleccionados = [];

    try {
        const codigo = encodeURIComponent(producto.codigoProducto);
        const response = await fetch(`/InventarioLotes/MostrarLotesPorProducto?filtroId=${codigo}`);

        MostrarBloqueDatosProducto(true);

        if (response.ok) {
            const lotes = await response.json();

            if (lotes.length > 0) {
                precioVentaBaseOriginal = producto.precioVentaXuni || 0;

                if (inputPrecioVentaPorUni) inputPrecioVentaPorUni.value = precioVentaBaseOriginal;

                if (typeof formatoMoneda === 'function') {
                    formatoMoneda(inputPrecioVentaPorUni, 'resPrecioVentaPorUni');
                }
            }
            lotesActuales = lotes;
            RenderizarLotes(lotes || []);
        } else {
            MostrarAlerta("info", "Stock - Lotes", "El producto No tiene lotes registrados.", 7000, false, null);
        }
    } catch (error) {
        console.error('Error en la petición de lotes:', error);
    }
}

async function CargarProductoExisteParaVenta(limpiarCampos) {
    let codigo = document.getElementById("validarCodigo")?.value;
    let productoEncontrado = false;

    if (limpiarCampos == true) {
        LimpiarCamposVenta();
    }

    if (!codigo) {
        console.log("El campo de código está vacío.");
        return;
    }

    try {
        const response = await fetch(`/Funciones/CargarProductoExisteParaVenta?filtroId=${codigo}`);

        if (response.ok) {
            const producto = await response.json();
            productoCargadoActual = producto;

            if (producto && Object.keys(producto).length > 0) {
                productoEncontrado = true;

                if (lblCantStockActual) {
                    lblCantStockActual.setAttribute("data-valor-interno", producto.cantStock);
                    lblCantStockActual.innerText = `${producto.cantStock} unidades en Stock`;
                }

                if (visualizaMarcaProducto) visualizaMarcaProducto.innerText = producto.nombreMarca || 'N/A';
                if (visualizaColorProducto) visualizaColorProducto.innerText = producto.nombreColor || 'N/A';
                if (visualizaNombreProducto) visualizaNombreProducto.innerText = producto.nombreProducto || 'N/A';
                if (visualizaReferenciaProducto) visualizaReferenciaProducto.innerText = producto.referencia || 'N/A';

                CargarLotesProductoVenta(producto);
                CargarImagenBase64(previewImagenProducto, producto.imagenProducto);

                if (ImagenProducto) ImagenProducto.value = producto.imagenProducto;
            } else {
                console.log("Producto no encontrado o datos vacíos.");
            }
        } else {
           console.log("No se encontró en la respuesta de la API el producto: ", codigo);
        }
    } catch (error) {
        console.error('Error al buscar producto:', error);
    }

    if (previewImagenProducto) {
        if (productoEncontrado && ImagenProducto?.value) {
            previewImagenProducto.style.display = 'block';
            if (iconoPreviewImagenProducto) iconoPreviewImagenProducto.style.display = 'none';
        } else {
            previewImagenProducto.style.display = 'none';
            if (iconoPreviewImagenProducto) iconoPreviewImagenProducto.style.display = 'block';
        }
    }
}

async function EliminarPedido(pedidoId) {
    if (!pedidoId || !tallerId) return;

    try {
        await fetch(`/Pedidos/EliminarPedido/${pedidoId}?filtroId=${tallerId}`, {
            method: 'DELETE'
        });
    } catch (e) {
        console.error("Error al eliminar el pedido en el rollback:", e);
    }
}

async function EliminarProductoEspecificoTabla(idProducto) {
    try {
        await fetch(`/InventarioSalidaProductos/EliminarPorId/${idProducto}?filtroId=${tallerId}`, {
            method: 'DELETE'
        });
    } catch (e) { console.error("Error al eliminar fila del producto", e); }
}

async function IniciarProcesoProducto(productoData) {
    //if (!tallerId || tallerId <= 0) {
    //    console.warn("Taller no válido.");
    //    return false;
    //}

    //if (!ConsecutivoActual) {
    //    console.warn("Pedido no válido.");
    //    return false;
    //}

    //if (!productoData?.lotes?.length) {
    //    console.warn("El producto no contiene información válida de lotes.");
    //    return false;
    //}

    //const productosInsertadosIds = [];
    //const lotesProcesados = [];
    //let stockActualizadoExitosamente = false;

    //// Función de rollback en BD si falla algún punto de la transacción
    //const ejecutarRollback = async () => {
    //    console.warn("Iniciando proceso de rollback para revertir operaciones en BD...");

    //    // 1. Revertir Stock General
    //    if (stockActualizadoExitosamente) {
    //        await RevertirStockVenta({
    //            CantVendidos: productoData.cantidad,
    //            CodigoProducto: productoData.codigoProducto,
    //            TallerId: parseInt(tallerId)
    //        });
    //    }

    //    // 2. Revertir Lotes procesados
    //    for (const lote of lotesProcesados) {
    //        await RevertirLoteVenta({
    //            LoteId: lote.loteId,
    //            CantVendidos: lote.cantidad,
    //            CodigoProducto: productoData.codigoProducto,
    //            TallerId: parseInt(tallerId)
    //        });
    //    }

    //    // 3. Eliminar Productos insertados de la tabla
    //    for (const idProd of productosInsertadosIds) {
    //        await EliminarProductoEspecificoTabla(idProd);
    //    }

    //    // 4. ELIMINAR EL PEDIDO CREADO Y REINICIAR CONSECUTIVO EN MEMORIA
    //    if (ConsecutivoActual) {
    //        await EliminarPedido(ConsecutivoActual);
    //        ConsecutivoActual = null;
    //        if (lblNumeroPedido) lblNumeroPedido.textContent = '';
    //    }
    //};

    try {
        //// 1. REGISTRAR EN TABLA, ACTUALIZAR LOTES Y GANANCIAS
        //for (const lote of productoData.lotes) {
        //    const dataProducto = {
        //        PrecioFinalXuni: productoData.precioUnitario,
        //        CantVendidos: lote.cantidad,
        //        CodigoProducto: productoData.codigoProducto,
        //        TallerId: parseInt(tallerId),
        //        ConsecutivoPedido: ConsecutivoActual,
        //        LoteId: lote.loteId
        //    };

        //    // A. Registrar Producto
        //    let idProductoInsertado = await AgregarProductoTabla(dataProducto);
        //    if (!idProductoInsertado) {
        //        MostrarAlerta("warning", "Error", `Error al registrar producto en Lote ${lote.loteId}.`, 10000);
        //        await ejecutarRollback();
        //        return false;
        //    }
        //    productosInsertadosIds.push(idProductoInsertado);

        //    // B. Actualizar Lote
        //    const dataLoteProducto = {
        //        LoteId: lote.loteId,
        //        CantVendidos: lote.cantidad,
        //        CodigoProducto: productoData.codigoProducto,
        //        TallerId: parseInt(tallerId)
        //    };

        //    let loteActualizado = await ActualizarLotesVenta(dataLoteProducto);
        //    if (!loteActualizado) {
        //        MostrarAlerta("warning", "Error Lote", `Error al actualizar stock del Lote ${lote.loteId}.`, 10000);
        //        await ejecutarRollback();
        //        return false;
        //    }
        //    lotesProcesados.push(lote);

        //    // C. Actualizar Ganancia por Lote
        //    const dataGananciaProducto = {
        //        TallerId: parseInt(tallerId),
        //        LoteId: lote.loteId,
        //        CodigoProducto: productoData.codigoProducto
        //    };

        //    let gananciaActualizada = await ActualizarGananciaVenta(dataGananciaProducto);
        //    if (!gananciaActualizada) {
        //        MostrarAlerta("warning", "Error Ganancia", `No se pudo registrar la ganancia del Lote ${lote.loteId}.`, 10000);
        //        await ejecutarRollback();
        //        return false;
        //    }
        //}

        //// 2. ACTUALIZAR STOCK GENERAL
        //const dataStockProducto = {
        //    CantVendidos: productoData.cantidad,
        //    CodigoProducto: productoData.codigoProducto,
        //    TallerId: parseInt(tallerId)
        //};

        //let stockActualizado = await ActualizarStockVenta(dataStockProducto);
        //if (!stockActualizado) {
        //    MostrarAlerta("warning", "Error Stock", "Error al actualizar stock general del producto.", 10000);
        //    await ejecutarRollback();
        //    return false;
        //}
        //stockActualizadoExitosamente = true;

        //MostrarAlerta("success", "Éxito", "Producto, lotes, stock y ganancias procesados correctamente.", 10000);
        //return true;

    } catch (error) {
        console.error("Error crítico durante la transacción:", error);
        await ejecutarRollback();
        MostrarAlerta("error", "Error Crítico", "Ocurrió una falla inesperada. Se han revertido las operaciones.", 10000);
        return false;
    }
}

////async function IniciarProcesoProducto(productoData) {
////    if (!tallerId || tallerId <= 0 || !ConsecutivoActual || !productoData?.lotes?.length) {
////        return false;
////    }

////    // Inicializar propiedades de control de estado en el mismo objeto del producto
////    productoData.idsInsertados = [];
////    productoData.lotesProcesados = [];
////    productoData.stockActualizado = false;

////    try {
////        // 1. Procesar cada lote
////        for (const lote of productoData.lotes) {

////            // Insertar Salida
////            const dataProducto = {
////                PrecioFinalXuni: productoData.precioUnitario,
////                CantVendidos: lote.cantidad,
////                CodigoProducto: productoData.codigoProducto,
////                TallerId: parseInt(tallerId, 10),
////                ConsecutivoPedido: ConsecutivoActual,
////                LoteId: lote.loteId
////            };

////            const idProductoInsertado = await AgregarProductoTabla(dataProducto);
////            if (!idProductoInsertado) return false;
////            productoData.idsInsertados.push(idProductoInsertado);

////            // Actualizar Lote
////            const dataLoteProducto = {
////                LoteId: lote.loteId,
////                CantVendidos: lote.cantidad,
////                CodigoProducto: productoData.codigoProducto,
////                TallerId: parseInt(tallerId, 10)
////            };

////            const loteActualizado = await ActualizarLotesVenta(dataLoteProducto);
////            if (!loteActualizado) return false;
////            productoData.lotesProcesados.push(lote);

////            // Actualizar Ganancia
////            const dataGananciaProducto = {
////                TallerId: parseInt(tallerId, 10),
////                LoteId: lote.loteId,
////                CodigoProducto: productoData.codigoProducto
////            };

////            const gananciaActualizada = await ActualizarGananciaVenta(dataGananciaProducto);
////            if (!gananciaActualizada) return false;
////        }

////        // 2. Actualizar Stock General
////        const dataStockProducto = {
////            CantVendidos: productoData.cantidad,
////            CodigoProducto: productoData.codigoProducto,
////            TallerId: parseInt(tallerId, 10)
////        };

////        const resStock = await ActualizarStockVenta(dataStockProducto);
////        if (!resStock) return false;

////        productoData.stockActualizado = true;

////        return true;

////    } catch (error) {
////        console.error("Error en IniciarProcesoProducto:", error);
////        return false;
////    }
////}

async function RegistrarMetodosPagoUnoPorUno(listaPagos, consecutivoPedido) {
    const pagosInsertados = [];

    for (const pago of listaPagos) {
        const payloadPago = {
            ValorPagado: pago.ValorPagado,
            EstadoPago: pago.EstadoPago || 'APLICADO',
            MetodoDePagoId: pago.MetodoPagoId,
            BancoId: pago.BancoId,
            TipoTarjetaPagoId: pago.TipoTarjetaId,
            ConsecutivoPedido: consecutivoPedido,
            TallerId: parseInt(tallerId, 10)
        };

        try {
            const response = await fetch(`/Pagos/AgregarPagoVenta?filtroId=${tallerId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(payloadPago)
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.error(`Error al registrar pago ${pago.MetodoPagoId}:`, errorText);
                return false;
            }
            pagosInsertados.push(pago);
        } catch (error) {
            console.error("Error de red al registrar método de pago:", error);
            return false;
        }
    }
    return true;
}

async function RegistrarVentaProducto() {
    var tipoDocumentoVenta = selectTipoDoc?.value;
    var documentoVenta = txtDocumento?.value?.trim();
    var primerNombreVenta = txtPrimerNombre?.value?.trim();
    var primerApellidoVenta = txtPrimerApellido?.value?.trim();
    var precioRestantePorPagar = QuitarFormatoMoneda(validarPrecioRestantePorPagar?.value?.trim());
    const productosInsertadosIds = [];
    const lotesProcesados = [];
    let stockActualizadoExitosamente = false;

    // 1. Validaciones iniciales
    if (precioRestantePorPagar > 0) {
        validarPrecioRestantePorPagar.focus();
        MostrarAlerta("warning", "Métodos de Pago", "Por favor completa el valor del pago total de la venta.", 10000);
        return;
    }
    if (!tipoDocumentoVenta) {
        selectTipoDoc.focus();
        MostrarAlerta("warning", "Datos cliente", "Por favor ingresa el tipo de documento del cliente.", 10000);
        return;
    }
    if (!documentoVenta) {
        txtDocumento.focus();
        txtDocumento.select();
        MostrarAlerta("warning", "Datos cliente", "Por favor ingresa el número de documento del cliente.", 10000);
        return;
    }
    if (!primerNombreVenta) {
        txtPrimerNombre.focus();
        txtPrimerNombre.select();
        MostrarAlerta("warning", "Datos cliente", "Por favor ingresa el primer nombre del cliente.", 10000);
        return;
    }
    if (!primerApellidoVenta) {
        txtPrimerApellido.focus();
        txtPrimerApellido.select();
        MostrarAlerta("warning", "Datos cliente", "Por favor ingresa el primer apellido del cliente.", 10000);
        return;
    }
    if (carritoVenta.length === 0) {
        if (validarCodigo) {
            validarCodigo.focus();
            validarCodigo.select();
        }
        MostrarAlerta("warning", "Datos Producto", "Por favor ingresa un producto a la venta.", 10000);
        return;
    }
    if (!ValidarMetodosPago()) {
        return;
    }
    if (!tallerId || tallerId <= 0) {
        console.warn("Taller no válido.");
        return false;
    }
    if (!ConsecutivoActual) {
        console.warn("Pedido no válido.");
        return false;
    }
    if (!carritoVenta[0].lotes?.length) {
        console.warn("El producto no contiene información válida de lotes.");
        return false;
    }


    const listaPagos = ObtenerMetodosPagoAgregados();
    if (listaPagos.length === 0) {
        MostrarAlerta("warning", "Método de Pago", "No se encontraron datos en los métodos de pago.", 10000);
        return;
    }

    // Array para llevar registro de los productos procesados exitosamente en BD para poder revertirlos si falla la factura o pagos
    //const productosProcesados = [];

    // Función de Rollback Total (Revierte Productos, Lotes, Stock, Ganancias, Pagos y Pedido)
    const ejecutarRollback = async () => {
        console.warn("Iniciando reversión total de la transacción...");

        // Revertir cada producto procesado
        for (const item of carritoVenta) {

            // Revertir Stock General
            if (stockActualizadoExitosamente) {
                await RevertirStockVenta({
                    CantVendidos: item.cantidad,
                    CodigoProducto: item.codigoProducto,
                    TallerId: parseInt(tallerId)
                });
            }

            // Revertir Lotes procesados
            for (const lote of lotesProcesados) {
                await RevertirLoteVenta({
                    LoteId: lote.loteId,
                    CantVendidos: lote.cantidad,
                    CodigoProducto: item.codigoProducto,
                    TallerId: parseInt(tallerId)
                });

                // Revertir Ganancia por Lote
                const dataGananciaProducto = {
                    TallerId: parseInt(tallerId),
                    LoteId: lote.loteId,
                    CodigoProducto: item.codigoProducto
                };
                await ActualizarGananciaVenta(dataGananciaProducto);
            }
        }

        if (ConsecutivoActual) {
            // Revertir pagos insertados
            await RevertirPagosVenta(ConsecutivoActual);

            // Eliminar Productos insertados de la tabla
            await EliminarProductoEspecificoTabla(ConsecutivoActual);

            // Eliminar el pedido
            await EliminarPedido(ConsecutivoActual);
            ConsecutivoActual = null;
            if (lblNumeroPedido) lblNumeroPedido.textContent = '';
        }
    };


    try {
        // REGISTRAR EN TABLA, ACTUALIZAR LOTES Y GANANCIAS
        for (const itemProducto of carritoVenta) {
            for (const lote of itemProducto.lotes) {
                const dataProducto = {
                    PrecioFinalXuni: itemProducto.precioUnitario,
                    CantVendidos: lote.cantidad,
                    CodigoProducto: itemProducto.codigoProducto,
                    TallerId: parseInt(tallerId),
                    ConsecutivoPedido: ConsecutivoActual,
                    LoteId: lote.loteId
                };

                // Registrar Producto
                let idProductoInsertado = await AgregarProductoTabla(dataProducto);
                if (!idProductoInsertado) {
                    MostrarAlerta("warning", "Error", `Error al registrar producto en Lote ${lote.loteId}.`, 10000);
                    await ejecutarRollback();
                    return false;
                }
                productosInsertadosIds.push(idProductoInsertado);

                // Actualizar Lote
                const dataLoteProducto = {
                    LoteId: lote.loteId,
                    CantVendidos: lote.cantidad,
                    CodigoProducto: itemProducto.codigoProducto,
                    TallerId: parseInt(tallerId)
                };

                let loteActualizado = await ActualizarLotesVenta(dataLoteProducto);
                if (!loteActualizado) {
                    MostrarAlerta("warning", "Error Lote", `Error al actualizar stock del Lote ${lote.loteId}.`, 10000);
                    await ejecutarRollback();
                    return false;
                }
                lotesProcesados.push(lote);

                // Actualizar Ganancia por Lote
                const dataGananciaProducto = {
                    TallerId: parseInt(tallerId),
                    LoteId: lote.loteId,
                    CodigoProducto: itemProducto.codigoProducto
                };

                let gananciaActualizada = await ActualizarGananciaVenta(dataGananciaProducto);
                if (!gananciaActualizada) {
                    MostrarAlerta("warning", "Error Ganancia", `No se pudo registrar la ganancia del Lote ${lote.loteId}.`, 10000);
                    await ejecutarRollback();
                    return false;
                }
            }

            // ACTUALIZAR STOCK GENERAL
            const dataStockProducto = {
                CantVendidos: itemProducto.cantidad,
                CodigoProducto: itemProducto.codigoProducto,
                TallerId: parseInt(tallerId)
            };

            let stockActualizado = await ActualizarStockVenta(dataStockProducto);
            if (!stockActualizado) {
                MostrarAlerta("warning", "Error Stock", "Error al actualizar stock general del producto.", 10000);
                await ejecutarRollback();
                return false;
            }
            stockActualizadoExitosamente = true;
        }


        // Registrar métodos de pago UNO A UNO
        const pagosExitosos = await RegistrarMetodosPagoUnoPorUno(listaPagos, ConsecutivoActual);
        if (!pagosExitosos) {
            MostrarAlerta("error", "Error Pagos", "No se pudieron registrar algunos métodos de pago. Revirtiendo operación...", 10000);
            await ejecutarRollback();
            return;
        }

        // Registrar Factura Completa
        const objetoDatosFactura = {
            DetalleObservacion: validarDetalle?.value?.trim() || '',
            TotalPagar: limpiarNumero(document.getElementById('lblTotalPagar')?.value),
            EstadoFactura: "FACTURADA",
            TallerId: parseInt(tallerId, 10),
            Cliente: {
                TipoDocumentoId: parseInt(tipoDocumentoVenta, 10),
                Documento: documentoVenta,
                PrimerNombre: primerNombreVenta,
                SegundoNombre: txtSegundoNombre?.value?.trim() || '',
                PrimerApellido: primerApellidoVenta,
                SegundoApellido: txtSegundoApellido?.value?.trim() || '',
                Telefono: txtTelefono?.value?.trim() || '',
                Correo: txtCorreo?.value?.trim() || '',
                EsConsumidorFinal: chkConsumidorFinal?.checked || false
            },
            Vehiculo: {
                TipoVehiculoId: selectTipoVehiculo?.value ? parseInt(selectTipoVehiculo.value, 10) : null,
                Placa: txtVehiculoPlaca?.value?.trim() || '',
                Modelo: txtVehiculoModelo?.value?.trim() || '',
                Kilometraje: txtVehiculoKm?.value ? parseInt(txtVehiculoKm.value, 10) : 0
            },
            ConsecutivoPedido: ConsecutivoActual
        };

        const responseVenta = await fetch(`/Facturas/RegistrarVentaFactura?filtroId=${tallerId}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
            body: JSON.stringify(objetoDatosFactura)
        });

        if (responseVenta.ok) {
            const dataRespuesta = await responseVenta.json();
            MostrarAlerta("success", "Venta Registrada", `Se registró la venta con factura: ${dataRespuesta.numeroFactura || ''} / pedido #: ${ConsecutivoActual}`, 10000);

            LimpiarCamposCliente();
            LimpiarCamposVenta();
            LimpiarFormularioVenta();
        } else {
            MostrarAlerta("error", "Error Venta", "No se pudo registrar la factura en la base de datos. Se han revertido los cambios.", 10000);
            await ejecutarRollback();
        }
    } catch (error) {
        console.error("Error al finalizar el registro de la venta:", error);
        await ejecutarRollback();
        MostrarAlerta("error", "Error Crítico", "Ocurrió una falla inesperada en el servidor. Se han revertido las operaciones.", 10000);
    }
}



////async function RegistrarVentaProducto() {
////    var tipoDocumentoVenta = selectTipoDoc?.value;
////    var documentoVenta = txtDocumento?.value?.trim();
////    var primerNombreVenta = txtPrimerNombre?.value?.trim();
////    var primerApellidoVenta = txtPrimerApellido?.value?.trim();
////    var precioRestantePorPagar = QuitarFormatoMoneda(validarPrecioRestantePorPagar?.value?.trim() || '0');

////    // Validaciones
////    if (precioRestantePorPagar > 0) {
////        validarPrecioRestantePorPagar?.focus();
////        MostrarAlerta("warning", "Métodos de Pago", "Por favor completa el valor del pago total de la venta.", 10000);
////        return;
////    }
////    if (!tipoDocumentoVenta || !documentoVenta || !primerNombreVenta || !primerApellidoVenta) {
////        MostrarAlerta("warning", "Datos cliente", "Por favor completa los datos obligatorios del cliente.", 10000);
////        return;
////    }
////    if (carritoVenta.length === 0) {
////        MostrarAlerta("warning", "Datos Producto", "Por favor ingresa un producto a la venta.", 10000);
////        return;
////    }
////    if (!ValidarMetodosPago()) return;

////    const listaPagos = ObtenerMetodosPagoAgregados();
////    if (listaPagos.length === 0) {
////        MostrarAlerta("warning", "Método de Pago", "No se encontraron datos en los métodos de pago.", 10000);
////        return;
////    }

////    // ARREGLOS PARA RASTREO
////    const productosProcesados = [];
////    let pagosRegistradosExitosamente = false;

////    try {
////        // PASO 1: Procesar Productos, Lotes, Stock y Ganancias
////        for (const item of carritoVenta) {
////            const ok = await IniciarProcesoProducto(item);

////            // Aunque falle el producto, lo agregamos para que el rollback limpie lo que se alcanzó a insertar
////            productosProcesados.push(item);

////            if (!ok) {
////                throw new Error(`Error procesando el producto ${item.codigoProducto}`);
////            }
////        }

////        // PASO 2: Registrar Métodos de Pago
////        const pagosExitosos = await RegistrarMetodosPagoUnoPorUno(listaPagos, ConsecutivoActual);
////        if (!pagosExitosos) {
////            throw new Error("Error al registrar los métodos de pago.");
////        }
////        pagosRegistradosExitosamente = true;

////        // PASO 3: Registrar la Factura Final
////        const objetoDatosFactura = {
////            DetalleObservacion: validarDetalle?.value?.trim() || '',
////            TotalPagar: limpiarNumero(document.getElementById('lblTotalPagar')?.value),
////            EstadoFactura: "FACTURADA",
////            TallerId: parseInt(tallerId, 10),
////            Cliente: {
////                TipoDocumentoId: parseInt(tipoDocumentoVenta, 10),
////                Documento: documentoVenta,
////                PrimerNombre: primerNombreVenta,
////                SegundoNombre: txtSegundoNombre?.value?.trim() || '',
////                PrimerApellido: primerApellidoVenta,
////                SegundoApellido: txtSegundoApellido?.value?.trim() || '',
////                Telefono: txtTelefono?.value?.trim() || '',
////                Correo: txtCorreo?.value?.trim() || '',
////                EsConsumidorFinal: chkConsumidorFinal?.checked || false
////            },
////            Vehiculo: {
////                TipoVehiculoId: selectTipoVehiculo?.value ? parseInt(selectTipoVehiculo.value, 10) : null,
////                Placa: txtVehiculoPlaca?.value?.trim() || '',
////                Modelo: txtVehiculoModelo?.value?.trim() || '',
////                Kilometraje: txtVehiculoKm?.value ? parseInt(txtVehiculoKm.value, 10) : 0
////            },
////            ConsecutivoPedido: ConsecutivoActual
////        };

////        const responseVenta = await fetch(`/Facturas/RegistrarVentaFactura?filtroId=${tallerId}`, {
////            method: 'POST',
////            headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
////            body: JSON.stringify(objetoDatosFactura)
////        });

////        if (!responseVenta.ok) {
////            throw new Error("Error al registrar la factura en el servidor.");
////        }

////        const dataRespuesta = await responseVenta.json();
////        MostrarAlerta("success", "Venta Registrada", `Se registró la venta con factura: ${dataRespuesta.numeroFactura || ''}`, 10000);

////        LimpiarCamposCliente();
////        LimpiarCamposVenta();
////        LimpiarFormularioVenta();

////    } catch (error) {
////        console.error("❌ Fallo detectado. Ejecutando Rollback de Venta...", error);

////        // EJECUCIÓN OBLIGATORIA DEL ROLLBACK CON TODO LO ACUMULADO
////        await RollbackVentaProducto(productosProcesados, pagosRegistradosExitosamente);

////        MostrarAlerta("error", "Venta Cancelada", `${error.message} Se han revertido los cambios en la base de datos.`, 10000);
////    }
////}

async function RevertirLoteVenta(dataLote) {
    try {
        await fetch(`/InventarioLotes/RevertirLoteVenta?filtroId=${tallerId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dataLote)
        });
    } catch (e) {
        console.error("Error al revertir lote", e);
    }
}

async function RevertirPagosVenta(consecutivoPedido) {
    if (!consecutivoPedido || !tallerId) return;

    try {
        await fetch(`/Pagos/EliminarPagosPorPedido/${consecutivoPedido}?filtroId=${tallerId}`, {
            method: 'DELETE'
        });
    } catch (e) {
        console.error("Error al revertir los pagos del pedido:", e);
    }
}

async function RevertirStockVenta(dataStock) {
    try {
        await fetch(`/InventarioStocks/RevertirStockVenta?filtroId=${tallerId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dataStock)
        });
    } catch (e) {
        console.error("Error al revertir stock", e);
    }
}

//async function RollbackVentaProducto(productosProcesados = [], pagosRegistrados = false) {
//    console.warn("⚠️ Ejecutando RollbackVentaProducto...");

//    try {
//        // 1. Revertir Pagos si alcanzaron a registrarse
//        if (pagosRegistrados && ConsecutivoActual) {
//            console.log("Revertiendo pagos...");
//            await RevertirPagosVenta(ConsecutivoActual);
//        }

//        // 2. Revertir Lotes, Ganancias, Stock y Eliminar Registros de Productos
//        for (const item of productosProcesados) {

//            // A. Revertir Lotes y Ganancias
//            if (Array.isArray(item.lotesProcesados) && item.lotesProcesados.length > 0) {
//                for (const lote of item.lotesProcesados) {
//                    console.log(`Revertiendo Lote ID: ${lote.loteId}`);
//                    await RevertirLoteVenta({
//                        LoteId: lote.loteId,
//                        CantVendidos: lote.cantidad,
//                        CodigoProducto: item.codigoProducto,
//                        TallerId: parseInt(tallerId, 10)
//                    });

//                    if (typeof RevertirGananciaVenta === 'function') {
//                        await RevertirGananciaVenta({
//                            TallerId: parseInt(tallerId, 10),
//                            LoteId: lote.loteId,
//                            CodigoProducto: item.codigoProducto
//                        });
//                    }
//                }
//            }

//            // B. Revertir Stock General si se actualizó para este producto
//            if (item.stockActualizado) {
//                console.log(`Revertiendo Stock de Producto: ${item.codigoProducto}`);
//                await RevertirStockVenta({
//                    CantVendidos: item.cantidad,
//                    CodigoProducto: item.codigoProducto,
//                    TallerId: parseInt(tallerId, 10)
//                });
//            }

//            // C. Eliminar registros creados en la tabla de salidas
//            if (Array.isArray(item.idsInsertados) && item.idsInsertados.length > 0) {
//                for (const idProd of item.idsInsertados) {
//                    console.log(`Eliminando Producto Salida ID: ${idProd}`);
//                    await EliminarProductoEspecificoTabla(idProd);
//                }
//            }
//        }

//        // 3. Eliminar Pedido
//        if (ConsecutivoActual) {
//            console.log(`Eliminando Pedido consecutivo: ${ConsecutivoActual}`);
//            await EliminarPedido(ConsecutivoActual);
//            ConsecutivoActual = null;
//            if (lblNumeroPedido) lblNumeroPedido.textContent = '';
//        }

//        console.log("✅ Rollback finalizado exitosamente.");

//    } catch (err) {
//        console.error("❌ Fallo crítico dentro del Rollback:", err);
//    }
//}



// ============================================================================
// 3. FUNCIONES SÍNCRONAS Y EVENT LISTENERS (Ordenadas Alfabéticamente)
// ============================================================================


function ActivarSeleccionLote(loteId, elementoHtml) {
    const index = lotesSeleccionados.indexOf(loteId);

    if (index > -1) {
        lotesSeleccionados.splice(index, 1);
        elementoHtml.classList.remove('btn-primary', 'active');
        elementoHtml.classList.add('btn-outline-primary');
    } else {
        const cantidadRequerida = parseInt(cantUnidadesVender?.value || 0, 10);

        if (cantidadRequerida <= 0) {
            Swal.fire({
                icon: 'warning',
                title: 'Atención',
                text: 'Ingresa primero la cantidad de unidades a vender.',
                timer: 5000,
                showConfirmButton: false
            });
            return;
        }

        let stockAcumulado = 0;
        document.querySelectorAll('.btn-lote.active').forEach(btn => {
            stockAcumulado += parseInt(btn.dataset.stock || 0, 10);
        });

        if (stockAcumulado >= cantidadRequerida) {
            Swal.fire({
                icon: 'info',
                title: 'Cantidad cubierta',
                text: `La cantidad requerida (${cantidadRequerida} und) ya está totalmente cubierta por los lotes seleccionados.`,
                timer: 5000,
                showConfirmButton: false
            });
            return;
        }

        lotesSeleccionados.push(loteId);
        elementoHtml.classList.remove('btn-outline-primary');
        elementoHtml.classList.add('btn-primary', 'active');
    }

    ActualizarCalculoUnidades();
}

function ActualizarCalculoUnidades() {
    const inputCantidad = document.getElementById('validarCantidadVendidos');
    let cantidadAVender = parseInt(inputCantidad?.value, 10);

    if (isNaN(cantidadAVender) || cantidadAVender <= 0) {
        cantidadAVender = 0;
    }

    const resumen = document.getElementById('resumenSeleccionLotes');
    if (!resumen) return;

    if (!lotesActuales || lotesActuales.length === 0) {
        return;
    }

    const stockTotalGlobal = lotesActuales.reduce((suma, lote) => {
        return suma + (lote.cantRestante ?? lote.stock ?? 0);
    }, 0);

    if (cantidadAVender > stockTotalGlobal) {
        cantidadAVender = stockTotalGlobal;
        if (inputCantidad) {
            inputCantidad.value = stockTotalGlobal;
        }
    }

    if (lotesSeleccionados.length === 0) {
        resumen.innerHTML = '<span class="text-warning fs-7">⚠️ Debe seleccionar al menos un lote para procesar la venta.</span>';
        return;
    }

    if (cantidadAVender === 0) {
        resumen.innerHTML = '<span class="text-secondary fs-7">Ingrese una cantidad válida mayor a 0.</span>';
        return;
    }

    const resultado = CalcularDistribucionLotes(cantidadAVender);

    let htmlResumen = '<div class="d-flex flex-wrap align-items-center gap-1 mb-1"><span class="fw-bold me-1 text-dark fs-7">Asignación:</span>';

    resultado.distribucion.forEach(item => {
        const badgeColor = item.tomadas > 0 ? 'bg-light text-primary border-primary' : 'bg-light text-muted border-secondary';

        htmlResumen += `
            <span class="badge ${badgeColor} border fs-7 me-1">
                Lote ${item.codigo}: <strong class="text-dark">${item.tomadas} und</strong>
            </span>
        `;
    });

    htmlResumen += '</div>';

    if (resultado.unidadesPendientes > 0) {
        htmlResumen += `<span class="text-danger fw-bold fs-7">⚠️ Faltan ${resultado.unidadesPendientes} unidades por cubrir. Seleccione más lotes.</span>`;
    } else {
        htmlResumen += `<span class="text-success fw-bold fs-7">✔ Cantidad requerida (${cantidadAVender} und) cubierta correctamente.</span>`;
    }

    resumen.innerHTML = htmlResumen;
}

function CalcularDistribucionLotes(cantidadRequerida) {
    if (!Array.isArray(lotesActuales) || lotesActuales.length === 0 || lotesSeleccionados.length === 0) {
        return { distribucion: [], unidadesPendientes: cantidadRequerida };
    }

    let unidadesPendientes = cantidadRequerida;
    const distribucion = [];

    for (const idSeleccionado of lotesSeleccionados) {
        const lote = lotesActuales.find(l => String(l.loteId ?? l.id) === String(idSeleccionado));

        if (!lote) continue;

        const loteId = lote.loteId ?? lote.id;
        const stockDisponible = lote.cantRestante ?? lote.stock ?? 0;

        const tomar = unidadesPendientes > 0 ? Math.min(stockDisponible, unidadesPendientes) : 0;
        unidadesPendientes -= tomar;

        distribucion.push({
            loteId: loteId,
            codigo: loteId,
            tomadas: tomar
        });
    }

    return { distribucion, unidadesPendientes };
}

function CalcularUnidadDesdeTotal() {
    const total = limpiarNumero(inputPrecioVentaTotal.value);
    const cantidad = parseInt(cantUnidadesVender.value, 10) || 0;

    if (cantidad > 0) {
        const precioUnidad = total / cantidad;
        inputPrecioVentaPorUni.value = SoloFormatoMoneda(precioUnidad);
    } else {
        inputPrecioVentaPorUni.value = SoloFormatoMoneda(0);
    }

    inputPrecioVentaTotal.value = SoloFormatoMoneda(total);
}

function CalcularVenta() {
    const stockActual = parseInt(lblCantStockActual?.getAttribute("data-valor-interno") || 0);
    const cantUniVender = parseFloat(cantUnidadesVender?.value) || 0;
    const precio = parseCurrency(inputPrecioVentaPorUni?.value);

    let total = 0;

    if (cantUniVender > stockActual) {
        total = stockActual * precio;
    } else {
        total = cantUniVender * precio;
    }

    if (inputPrecioVentaPorUni) {
        inputPrecioVentaPorUni.value = SoloFormatoMoneda(precio);
    }

    if (inputPrecioVentaTotal) {
        inputPrecioVentaTotal.value = SoloFormatoMoneda(total);
    }
}

function EliminarItemCarrito(index) {
    carritoVenta.splice(index, 1);
    RenderizarTablaVenta();
    if (typeof calcularTotalPagado_Y_Restante === 'function') {
        calcularTotalPagado_Y_Restante(null, null, null);
    }
}

function LimpiarCamposCliente() {
    const idsInputs = [
        'txtClienteDocumento',
        'txtPrimerNombre',
        'txtSegundoNombre',
        'txtPrimerApellido',
        'txtSegundoApellido',
        'txtClienteTelefono',
        'txtCorreo',
        'txtVehiculoPlaca',
        'txtVehiculoModelo',
        'txtVehiculoKm'
    ];

    if (chkConsumidorFinal) chkConsumidorFinal.checked = false;

    idsInputs.forEach(id => {
        const input = document.getElementById(id);
        if (input) {
            input.value = '';
            input.readOnly = false;
        }
    });

    if (selectTipoDoc) {
        selectTipoDoc.value = '';
        selectTipoDoc.style.pointerEvents = '';
        selectTipoDoc.style.backgroundColor = '';
        selectTipoDoc.removeAttribute('tabindex');
    }

    if (selectTipoVehiculo) {
        selectTipoVehiculo.value = '';
        selectTipoVehiculo.style.pointerEvents = '';
        selectTipoVehiculo.style.backgroundColor = '';
        selectTipoVehiculo.removeAttribute('tabindex');
    }

    if (btnBuscar) btnBuscar.disabled = false;
}

function LimpiarCamposVenta() {
    lotesSeleccionados = [];
    lotesActuales = null;
    productoCargadoActual = null;

    if (lblCantStockActual) lblCantStockActual.setAttribute("data-valor-interno", '');
    if (inputPrecioVentaPorUni) inputPrecioVentaPorUni.value = '';
    if (inputPrecioVentaTotal) inputPrecioVentaTotal.value = '';
    if (lblCantStockActual) lblCantStockActual.textContent = '';
    if (cantUnidadesVender) cantUnidadesVender.value = '';
    if (chkManualTotal) chkManualTotal.checked = false;
    if (chkConsumidorFinal) chkConsumidorFinal.checked = false;
    if (precioVentaBaseOriginal) precioVentaBaseOriginal = 0;

    if (visualizaMarcaProducto) visualizaMarcaProducto.innerText = '';
    if (visualizaNombreProducto) visualizaNombreProducto.innerText = '';
    if (visualizaReferenciaProducto) visualizaReferenciaProducto.innerText = '';
    if (visualizaColorProducto) visualizaColorProducto.innerText = '';

    if (iconoPreviewImagenProducto) iconoPreviewImagenProducto.style.display = 'block';

    if (previewImagenProducto) {
        previewImagenProducto.style.display = 'none';
        previewImagenProducto.value = null;
    }
    if (ImagenProducto) ImagenProducto.value = null;

    const resumen = document.getElementById('resumenSeleccionLotes');
    if (resumen) resumen.innerHTML = '';

    LimpiarLotesCompletamente();
    MostrarBloqueDatosProducto(false);
}

function LimpiarFormularioVenta() {
    // Limpiar productos
    const tbody = document.getElementById('tbodyDetalleVenta');
    const lblTotalPagar = document.getElementById('lblTotalPagar');

    if (carritoVenta.length > 0) {
        tbody.innerHTML = `
            <tr id="trFilaVacia">
                <td colspan="6" class="text-center text-muted py-4">No hay productos agregados a la venta.</td>
            </tr>`;
        if (lblTotalPagar) lblTotalPagar.value = SoloFormatoMoneda(0);
        if (validarPrecioTotalPagado) validarPrecioTotalPagado.value = SoloFormatoMoneda(0);
        if (validarPrecioRestantePorPagar) validarPrecioRestantePorPagar.value = SoloFormatoMoneda(0);
    }

    //Limpiar detalle
    if (validarDetalle) {
        validarDetalle.value = "";
    }

    //Limpiar pagos
    const contenedorPagos = document.getElementById('paymentMethodsContainer');
    const cantidadPagos = contenedorPagos ? contenedorPagos.children.length : 0;
    if (cantidadPagos > 0) {
        if (contenedorPagos) {
            contenedorPagos.innerHTML = "";
        }
    }

    carritoVenta = [];
    ConsecutivoActual = null;
    lblNumeroPedido.textContent = null;
}

function LimpiarLotesCompletamente() {
    lotesSeleccionados = [];

    const contenedorLotes = document.getElementById('contenedorLotes');

    if (contenedorLotes) {
        contenedorLotes.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
            const instance = bootstrap.Tooltip.getInstance(el);
            if (instance) instance.dispose();
        });

        contenedorLotes.innerHTML = '';
    }

    const contenedorAsignacion = document.getElementById('contenedorAsignacion');
    if (contenedorAsignacion) {
        contenedorAsignacion.innerHTML = '';
    }

    if (typeof ActualizarCalculoUnidades === 'function') {
        ActualizarCalculoUnidades();
    }
}

function MostrarBloqueDatosProducto(mostrar) {
    const contenedor = document.getElementById('contenedorDetallesProducto');
    if (mostrar) {
        if (contenedor) {
            contenedor.classList.remove('d-none');
            contenedor.removeAttribute('aria-hidden');
        }
    } else {
        if (contenedor) {
            contenedor.classList.add('d-none');
            contenedor.setAttribute('aria-hidden', 'true');
        }
    }
}

function ObtenerMetodosPagoAgregados() {
    const contenedorPagos = document.getElementById('paymentMethodsContainer');
    const metodosPago = [];

    if (!contenedorPagos) return metodosPago;

    // Se obtienen las tarjetas de pago insertadas a partir del template
    const tarjetasPago = contenedorPagos.querySelectorAll('.payment-card, .active-payment-block');

    tarjetasPago.forEach(tarjeta => {
        // Seleccionamos los elementos usando las clases reales del template
        const selectMetodo = tarjeta.querySelector('.metodo-pago-select');
        const selectBanco = tarjeta.querySelector('.banco-select');
        const selectTipoTarjeta = tarjeta.querySelector('.tipo-tarjeta-select');
        const inputMonto = tarjeta.querySelector('.monto-pago');

        if (selectMetodo && inputMonto) {
            const metodoPagoId = parseInt(selectMetodo.value, 10) || 0;
            // Para input type="number", parseFloat obtiene directamente el decimal correcto
            const valorPagado = parseFloat(inputMonto.value) || 0;

            if (metodoPagoId > 0 && valorPagado > 0) {
                metodosPago.push({
                    MetodoPagoId: metodoPagoId,
                    BancoId: selectBanco ? (parseInt(selectBanco.value, 10) || null) : null,
                    TipoTarjetaId: selectTipoTarjeta ? (parseInt(selectTipoTarjeta.value, 10) || null) : null,
                    ValorPagado: valorPagado
                });
            }
        }
    });

    return metodosPago;
}

function RenderizarLotes(lotes) {
    const contenedor = document.getElementById('contenedorLotes');
    if (!contenedor) return;

    contenedor.innerHTML = '';

    if (!lotes || lotes.length === 0) {
        contenedor.innerHTML = '<span class="badge bg-danger">Sin stock de lotes</span>';
        MostrarAlerta("info", "Stock - Lotes", "El producto No tiene lotes registrados.", 7000, false, null);
        return;
    }

    lotes.forEach(lote => {
        const loteId = lote.loteId ?? lote.loteId;
        const stock = lote.cantRestante ?? lote.CantRestante ?? 0;
        const precioCompra = lote.precioCompraXuni ?? lote.PrecioCompraXuni ?? 0;
        const precioVenta = lote.precioVentaXuni ?? lote.PrecioVentaXuni ?? 0;
        const loteIdSanitizado = String(loteId).replace(/[^\w-]/g, '');

        let precioCompraP = SoloFormatoMoneda(precioCompra);
        let precioVentaP = SoloFormatoMoneda(precioVenta);

        const btnLote = document.createElement('button');
        btnLote.type = 'button';
        btnLote.className = 'btn btn-outline-primary btn-sm rounded-pill position-relative me-1 mb-1 btn-lote';
        btnLote.dataset.loteId = loteIdSanitizado;
        btnLote.dataset.stock = stock;
        btnLote.dataset.precio = precioVenta;

        btnLote.innerHTML = `
                Lote <strong>${loteId}</strong> 
                <span class="badge bg-secondary ms-1">${stock} und</span>
                <span class="badge bg-danger ms-1 badge-venta-tooltip"
                      data-bs-toggle="tooltip"
                      data-bs-placement="top"
                      data-bs-title="Venta: ${precioVentaP}" 
                      data-bs-trigger="click"
                      style="cursor: pointer;">
                    <i class="fa fa-low-vision" aria-hidden="true"></i>
                </span>
                <span class="badge bg-success ms-1 badge-compra-tooltip"
                      data-bs-toggle="tooltip"
                      data-bs-placement="top"
                      data-bs-title="Compra: ${precioCompraP}" 
                      data-bs-trigger="click"
                      style="cursor: pointer;">
                    <i class="fa fa-low-vision" aria-hidden="true"></i>
                </span>
            `;

        btnLote.addEventListener('click', () => ActivarSeleccionLote(loteIdSanitizado, btnLote));
        contenedor.appendChild(btnLote);

        btnLote.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
            const tooltip = new bootstrap.Tooltip(el);
            let timer;

            el.addEventListener('click', (e) => e.stopPropagation());

            el.addEventListener('shown.bs.tooltip', () => {
                clearTimeout(timer);
                timer = setTimeout(() => tooltip.hide(), 4000);
            });
        });
    });
    ActualizarCalculoUnidades();
}

function RenderizarTablaVenta() {
    const tbody = document.getElementById('tbodyDetalleVenta');
    const lblTotalPagar = document.getElementById('lblTotalPagar');

    if (!tbody) return;

    if (carritoVenta.length === 0) {
        tbody.innerHTML = `
            <tr id="trFilaVacia">
                <td colspan="6" class="text-center text-muted py-4">No hay productos agregados a la venta.</td>
            </tr>`;
        if (lblTotalPagar) lblTotalPagar.value = SoloFormatoMoneda(0);
        return;
    }

    let html = '';
    let granTotal = 0;

    carritoVenta.forEach((item, index) => {
        granTotal += item.total;

        const textoLotes = (item.lotes && item.lotes.length > 0)
            ? item.lotes.map(l => `${l.referencia} (${l.cantidad} und)`).join(', ')
            : 'Sin lote asignado';

        html += `
            <tr>
                <td class="text-center fw-bold fs-7">${item.codigoProducto}</td>
                <td>
                    <div class="fw-bold">${item.nombreProducto}</div>
                    <small class="text-muted fs-8 d-block">
                        Lotes: ${textoLotes}
                    </small>
                </td>
                <td class="text-center fw-bold">${item.cantidad}</td>
                <td class="text-end">${SoloFormatoMoneda(item.precioUnitario)}</td>
                <td class="text-end fw-bold text-success">${SoloFormatoMoneda(item.total)}</td>
                <td class="text-center">
                    <button class="btn btn-outline-danger btn-sm border-0" onclick="EliminarItemCarrito(${index})">
                        <i class="fa-solid fa-trash-arrow-up"></i> Quitar
                    </button>
                </td>
            </tr>`;
    });

    tbody.innerHTML = html;
    if (lblTotalPagar) lblTotalPagar.value = SoloFormatoMoneda(granTotal);
}

function ValidarMetodosPago() {
    const contenedorPagos = document.getElementById('paymentMethodsContainer');
    const cantidadPagos = contenedorPagos ? contenedorPagos.children.length : 0;

    if (cantidadPagos === 0) {
        MostrarAlerta("warning", "Método de Pago", "Debe agregar al menos un método de pago para completar la venta.", 10000);

        const btnAgregarPago = document.getElementById('addPaymentMethod');
        if (btnAgregarPago) {
            btnAgregarPago.focus();
        }
        return false;
    }
    return true;
}



// ----------------------------------------------------------------------------
// EVENT LISTENERS
// ----------------------------------------------------------------------------

if (cantUnidadesVender) {
    cantUnidadesVender.addEventListener('input', function () {
        if (chkManualTotal?.checked) {
            CalcularUnidadDesdeTotal();
        } else if (typeof ActualizarCalculoUnidades === 'function') {
            ActualizarCalculoUnidades();
        }
    });
}

if (chkConsumidorFinal) {
    chkConsumidorFinal.addEventListener('change', function () {
        const esConsumidorFinal = this.checked;

        const inputsCliente = [
            txtDocumento,
            txtPrimerNombre,
            txtSegundoNombre,
            txtPrimerApellido,
            txtSegundoApellido,
            txtTelefono,
            txtCorreo,
            selectTipoVehiculo,
            txtVehiculoPlaca,
            txtVehiculoModelo,
            txtVehiculoKm
        ];

        if (esConsumidorFinal) {
            selectTipoDoc_antes = selectTipoDoc?.value;
            txtDocumento_antes = txtDocumento?.value;
            txtPrimerNombre_antes = txtPrimerNombre?.value;
            txtSegundoNombre_antes = txtSegundoNombre?.value;
            txtPrimerApellido_antes = txtPrimerApellido?.value;
            txtSegundoApellido_antes = txtSegundoApellido?.value;
            txtTelefono_antes = txtTelefono?.value;
            txtCorreo_antes = txtCorreo?.value;
            txtVehiculoPlaca_antes = txtVehiculoPlaca?.value;
            selectTipoVehiculo_antes = selectTipoVehiculo?.value;
            txtVehiculoModelo_antes = txtVehiculoModelo?.value;
            txtVehiculoKm_antes = txtVehiculoKm?.value;

            if (selectTipoDoc) selectTipoDoc.value = '1';
            if (txtDocumento) txtDocumento.value = '222222222222';
            if (txtPrimerNombre) txtPrimerNombre.value = 'Clientes';
            if (txtSegundoNombre) txtSegundoNombre.value = '';
            if (txtPrimerApellido) txtPrimerApellido.value = 'Varios';
            if (txtSegundoApellido) txtSegundoApellido.value = '';
            if (txtTelefono) txtTelefono.value = '0000000000';
            if (txtCorreo) txtCorreo.value = '';
            if (selectTipoVehiculo) selectTipoVehiculo.value = '';
            if (txtVehiculoPlaca) txtVehiculoPlaca.value = '';
            if (txtVehiculoModelo) txtVehiculoModelo.value = '';
            if (txtVehiculoKm) txtVehiculoKm.value = '';

            inputsCliente.forEach(input => {
                if (input) input.readOnly = true;
            });

            if (selectTipoDoc) {
                selectTipoDoc.style.pointerEvents = 'none';
                selectTipoDoc.style.backgroundColor = '#e9ecef';
                selectTipoDoc.tabIndex = -1;
            }

            if (selectTipoVehiculo) {
                selectTipoVehiculo.style.pointerEvents = 'none';
                selectTipoVehiculo.style.backgroundColor = '#e9ecef';
                selectTipoVehiculo.tabIndex = -1;
            }

            if (btnBuscar) btnBuscar.disabled = true;

        } else {
            LimpiarCamposCliente();
            if (selectTipoDoc) selectTipoDoc.value = selectTipoDoc_antes;
            if (txtDocumento) txtDocumento.value = txtDocumento_antes;
            if (txtPrimerNombre) txtPrimerNombre.value = txtPrimerNombre_antes;
            if (txtSegundoNombre) txtSegundoNombre.value = txtSegundoNombre_antes;
            if (txtPrimerApellido) txtPrimerApellido.value = txtPrimerApellido_antes;
            if (txtSegundoApellido) txtSegundoApellido.value = txtSegundoApellido_antes;
            if (txtTelefono) txtTelefono.value = txtTelefono_antes;
            if (txtCorreo) txtCorreo.value = txtCorreo_antes;
            if (txtVehiculoPlaca) txtVehiculoPlaca.value = txtVehiculoPlaca_antes;
            if (selectTipoVehiculo) selectTipoVehiculo.value = selectTipoVehiculo_antes;
            if (txtVehiculoModelo) txtVehiculoModelo.value = txtVehiculoModelo_antes;
            if (txtVehiculoKm) txtVehiculoKm.value = txtVehiculoKm_antes;
        }
    });
}

if (chkManualTotal) {
    chkManualTotal.addEventListener('change', function () {
        if (this.checked) {
            inputPrecioVentaTotal.removeAttribute('readonly');
            inputPrecioVentaTotal.classList.add('bg-white');
            inputPrecioVentaTotal.focus();

            if (inputPrecioVentaPorUni) {
                inputPrecioVentaPorUni.readOnly = true;
                inputPrecioVentaPorUni.classList.add('bg-light');
            }
            CalcularUnidadDesdeTotal();
        } else {
            inputPrecioVentaTotal.readOnly = true;
            inputPrecioVentaTotal.classList.remove('bg-white');

            if (inputPrecioVentaPorUni) {
                inputPrecioVentaPorUni.readOnly = false;
                inputPrecioVentaPorUni.classList.remove('bg-light');

                inputPrecioVentaPorUni.value = precioVentaBaseOriginal;

                if (typeof formatoMoneda === 'function') {
                    formatoMoneda(inputPrecioVentaPorUni, 'resPrecioVentaPorUni');
                }

                CalcularVenta();
            }

            if (typeof ActualizarCalculoUnidades === 'function') {
                ActualizarCalculoUnidades();
            }
        }
    });
}

if (inputPrecioVentaTotal) {
    inputPrecioVentaTotal.addEventListener('input', function () {
        if (chkManualTotal?.checked) {
            CalcularUnidadDesdeTotal();
        }
    });

    inputPrecioVentaTotal.addEventListener('blur', function () {
        if (chkManualTotal?.checked) {
            const num = limpiarNumero(this.value);
            this.value = SoloFormatoMoneda(num);
        }
    });
}



//function alCargarProductoExitoso(producto) {
//    productoCargadoActual = producto;
//    if (visualizaMarcaProducto) visualizaMarcaProducto.textContent = producto.marca;
//    if (visualizaNombreProducto) visualizaNombreProducto.textContent = producto.nombre;
//}

////async function RegistrarVentaProducto() {
////    const tipoDocumentoVenta = selectTipoDoc?.value;
////    const documentoVenta = txtDocumento?.value?.trim();
////    const primerNombreVenta = txtPrimerNombre?.value?.trim();
////    const primerApellidoVenta = txtPrimerApellido?.value?.trim();

////    // 1. Validaciones de Cliente
////    if (!tipoDocumentoVenta) {
////        selectTipoDoc.focus();
////        MostrarAlerta("warning", "Datos cliente", "Por favor ingresa el tipo de documento del cliente.", 10000);
////        return;
////    }
////    if (!documentoVenta) {
////        txtDocumento.focus();
////        txtDocumento.select();
////        MostrarAlerta("warning", "Datos cliente", "Por favor ingresa el número de documento del cliente.", 10000);
////        return;
////    }
////    if (!primerNombreVenta) {
////        txtPrimerNombre.focus();
////        txtPrimerNombre.select();
////        MostrarAlerta("warning", "Datos cliente", "Por favor ingresa el primer nombre del cliente.", 10000);
////        return;
////    }
////    if (!primerApellidoVenta) {
////        txtPrimerApellido.focus();
////        txtPrimerApellido.select();
////        MostrarAlerta("warning", "Datos cliente", "Por favor ingresa el primer apellido del cliente.", 10000);
////        return;
////    }

////    // 2. Validación de Carrito
////    if (carritoVenta.length === 0) {
////        validarCodigo?.focus();
////        validarCodigo?.select();
////        MostrarAlerta("warning", "Datos Producto", "Por favor ingresa al menos un producto a la venta.", 10000);
////        return;
////    }

////    // 3. Validación de Métodos de Pago
////    if (!ValidarMetodosPago()) return;

////    const listaPagos = ObtenerMetodosPagoAgregados();
////    if (listaPagos.length === 0) {
////        MostrarAlerta("warning", "Método de Pago", "No se encontraron montos válidos en los métodos de pago.", 10000);
////        return;
////    }

////    // 4. Payload Maestro de la Venta / Cliente / Vehículo
////    const objetoDatosFactura = {
////        TallerId: parseInt(tallerId, 10),
////        ConsecutivoPedido: ConsecutivoActual,
////        Cliente: {
////            TipoDocumentoId: parseInt(tipoDocumentoVenta, 10),
////            Documento: documentoVenta,
////            PrimerNombre: primerNombreVenta,
////            SegundoNombre: txtSegundoNombre?.value?.trim() || '',
////            PrimerApellido: primerApellidoVenta,
////            SegundoApellido: txtSegundoApellido?.value?.trim() || '',
////            Telefono: txtTelefono?.value?.trim() || '',
////            Correo: txtCorreo?.value?.trim() || '',
////            EsConsumidorFinal: chkConsumidorFinal?.checked || false
////        },
////        Vehiculo: {
////            TipoVehiculoId: selectTipoVehiculo?.value ? parseInt(selectTipoVehiculo.value, 10) : null,
////            Placa: txtVehiculoPlaca?.value?.trim() || '',
////            Modelo: txtVehiculoModelo?.value?.trim() || '',
////            Kilometraje: txtVehiculoKm?.value ? parseInt(txtVehiculoKm.value, 10) : 0
////        },
////        DetalleObservacion: validarDetalle?.value?.trim() || '',
////        TotalPagar: limpiarNumero(document.getElementById('lblTotalPagar')?.value)
////    };

////    try {
////        // Step A: Registrar Cabecera/Maestro de Venta
////        const responseVenta = await fetch(`/Ventas/RegistrarVentaMaestro?filtroId=${tallerId}`, {
////            method: 'POST',
////            headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
////            body: JSON.stringify(objetoDatosFactura)
////        });

////        if (!responseVenta.ok) {
////            MostrarAlerta("error", "Error Venta", "No se pudo registrar la cabecera de la venta.", 10000);
////            return;
////        }

////        // Step B: Registrar Métodos de Pago UNO A UNO en la tabla de pagos
////        const pagosExitosos = await RegistrarMetodosPagoUnoPorUno(listaPagos, ConsecutivoActual);

////        if (pagosExitosos) {
////            MostrarAlerta("success", "Venta Registrada", `Venta y métodos de pago registrados correctamente. Pedido #: ${ConsecutivoActual}`, 10000);

////            LimpiarCamposCliente();
////            LimpiarCamposVenta();
////            LimpiarFormularioVenta();
////        } else {
////            MostrarAlerta("warning", "Atención", "La venta se guardó pero ocurrió un error registrando algunos pagos.", 10000);
////        }

////    } catch (error) {
////        console.error("Error al procesar la venta:", error);
////        MostrarAlerta("error", "Error Crítico", "Ocurrió un fallo en el servidor durante la operación.", 10000);
////    }
////}






















//// Variable global o de ámbito de la venta
//let lotesActuales = null;
//let lotesSeleccionados = []; // Guarda los IDs de los lotes activos en orden de clic
//let precioVentaBaseOriginal = 0;
//let ImagenProducto = document.getElementById('ImagenProducto');
//let previewImagenProducto = document.getElementById('previewImagenProducto');
//const tallerId = document.getElementById("txtTallerId").value;

//// CORREGIDO: Se declara la variable a nivel global para que sea accesible en todo el script
//const iconoPreviewImagenProducto = document.getElementById('iconoPreviewImagenProducto');
//const validarCodigo = document.getElementById("validarCodigo");

//let carritoVenta = [];
//let productoCargadoActual = null;

//const visualizaMarcaProducto = document.getElementById('lblMarcaProducto');
//const visualizaNombreProducto = document.getElementById('lblNombreProducto');
//const visualizaReferenciaProducto = document.getElementById('lblReferenciaProducto');
//const visualizaColorProducto = document.getElementById('lblColorProducto');
//const lblCantStockActual = document.getElementById('lblCantStockActual');
//let inputPrecioVentaPorUni = document.getElementById('validarPrecioVentaPorUni');
//const cantUnidadesVender = document.getElementById('validarCantidadVendidos');
//const chkManualTotal = document.getElementById('chkManualTotal');
//const chkConsumidorFinal = document.getElementById('chkConsumidorFinal');
//const inputPrecioVentaTotal = document.getElementById('validarPrecioVentaTotal');

//// Referencias a los elementos del formulario
//const selectTipoDoc = document.getElementById('TipoDocumentoId');
//const txtDocumento = document.getElementById('txtClienteDocumento');
//const txtPrimerNombre = document.getElementById('txtPrimerNombre');
//const txtSegundoNombre = document.getElementById('txtSegundoNombre');
//const txtPrimerApellido = document.getElementById('txtPrimerApellido');
//const txtSegundoApellido = document.getElementById('txtSegundoApellido');
//const txtTelefono = document.getElementById('txtClienteTelefono');
//const txtCorreo = document.getElementById('txtCorreo');
//const txtVehiculoPlaca = document.getElementById('txtVehiculoPlaca');
//const selectTipoVehiculo = document.getElementById('TipoVehiculoId');
//const txtVehiculoModelo = document.getElementById('txtVehiculoModelo');
//const txtVehiculoKm = document.getElementById('txtVehiculoKm');
//const btnBuscar = document.getElementById('btnBuscarCliente');
//const lblNumeroPedido = document.getElementById('lblNumeroPedido');
//const validarDetalle = document.getElementById('validarDetalle');

////let IdConsecutivoActual = null;
//let ConsecutivoActual = null;

//let selectTipoDoc_antes = null;
//let txtDocumento_antes = null;
//let txtPrimerNombre_antes = null;
//let txtSegundoNombre_antes = null;
//let txtPrimerApellido_antes = null;
//let txtSegundoApellido_antes = null;
//let txtTelefono_antes = null;
//let txtCorreo_antes = null;
//let selectTipoVehiculo_antes = null;
//let txtVehiculoPlaca_antes = null;
//let txtVehiculoModelo_antes = null;
//let txtVehiculoKm_antes = null;



///**
// * 1. Carga los lotes desde el Backend filtrando por código de producto
// */
//async function CargarLotesProductoVenta(producto) {
//    if (!producto || !producto.codigoProducto) {
//        console.warn('El producto ingresado no contiene un código válido.');
//        return;
//    }

//    lotesSeleccionados = [];

//    try {
//        const codigo = encodeURIComponent(producto.codigoProducto);
//        const response = await fetch(`/InventarioLotes/MostrarLotesPorProducto?filtroId=${codigo}`);

//        MostrarBloqueDatosProducto(true);

//        if (response.ok) {
//            const lotes = await response.json();

//            if (lotes.length > 0) {
//                precioVentaBaseOriginal = producto.precioVentaXuni || 0;

//                if (inputPrecioVentaPorUni) inputPrecioVentaPorUni.value = precioVentaBaseOriginal;

//                if (typeof formatoMoneda === 'function') {
//                    formatoMoneda(inputPrecioVentaPorUni, 'resPrecioVentaPorUni');
//                }
//            }
//            lotesActuales = lotes;
//            RenderizarLotes(lotes || []);
//        } else {
//            MostrarAlerta("info", "Stock - Lotes", "El producto No tiene lotes registrados.", 7000, false, null);
//        }
//    } catch (error) {
//        console.error('Error en la petición de lotes:', error);
//    }
//}

///**
// * 2. Renderiza la fila de lotes adaptada a las columnas de BD
// */
//function RenderizarLotes(lotes) {
//    const contenedor = document.getElementById('contenedorLotes');
//    if (!contenedor) return;

//    contenedor.innerHTML = '';

//    if (!lotes || lotes.length === 0) {
//        contenedor.innerHTML = '<span class="badge bg-danger">Sin stock de lotes</span>';
//        MostrarAlerta("info", "Stock - Lotes", "El producto No tiene lotes registrados.", 7000, false, null);
//        return;
//    }

//    lotes.forEach(lote => {
//        const loteId = lote.loteId ?? lote.loteId;
//        const stock = lote.cantRestante ?? lote.CantRestante ?? 0;
//        const precioCompra = lote.precioCompraXuni ?? lote.PrecioCompraXuni ?? 0;
//        const precioVenta = lote.precioVentaXuni ?? lote.PrecioVentaXuni ?? 0;
//        const loteIdSanitizado = String(loteId).replace(/[^\w-]/g, '');

//        let precioCompraP = SoloFormatoMoneda(precioCompra);
//        let precioVentaP = SoloFormatoMoneda(precioVenta);

//        const btnLote = document.createElement('button');
//        btnLote.type = 'button';
//        btnLote.className = 'btn btn-outline-primary btn-sm rounded-pill position-relative me-1 mb-1 btn-lote';
//        btnLote.dataset.loteId = loteIdSanitizado;
//        btnLote.dataset.stock = stock;
//        btnLote.dataset.precio = precioVenta;

//        btnLote.innerHTML = `
//                Lote <strong>${loteId}</strong>
//                <span class="badge bg-secondary ms-1">${stock} und</span>
//                <span class="badge bg-danger ms-1 badge-venta-tooltip"
//                      data-bs-toggle="tooltip"
//                      data-bs-placement="top"
//                      data-bs-title="Venta: ${precioVentaP}"
//                      data-bs-trigger="click"
//                      style="cursor: pointer;">
//                    <i class="fa fa-low-vision" aria-hidden="true"></i>
//                </span>
//                <span class="badge bg-success ms-1 badge-compra-tooltip"
//                      data-bs-toggle="tooltip"
//                      data-bs-placement="top"
//                      data-bs-title="Compra: ${precioCompraP}"
//                      data-bs-trigger="click"
//                      style="cursor: pointer;">
//                    <i class="fa fa-low-vision" aria-hidden="true"></i>
//                </span>
//            `;

//        btnLote.addEventListener('click', () => ActivarSeleccionLote(loteIdSanitizado, btnLote));
//        contenedor.appendChild(btnLote);

//        btnLote.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
//            const tooltip = new bootstrap.Tooltip(el);
//            let timer;

//            el.addEventListener('click', (e) => e.stopPropagation());

//            el.addEventListener('shown.bs.tooltip', () => {
//                clearTimeout(timer);
//                timer = setTimeout(() => tooltip.hide(), 4000);
//            });
//        });
//    });
//    ActualizarCalculoUnidades();
//}

///**
// * 3. Manejo de selección/deselección de lotes manteniendo orden de clic
// * CORREGIDO: Se eliminó la función duplicada
// */
//function ActivarSeleccionLote(loteId, elementoHtml) {
//    const index = lotesSeleccionados.indexOf(loteId);

//    if (index > -1) {
//        lotesSeleccionados.splice(index, 1);
//        elementoHtml.classList.remove('btn-primary', 'active');
//        elementoHtml.classList.add('btn-outline-primary');
//    } else {
//        const cantidadRequerida = parseInt(cantUnidadesVender?.value || 0, 10);

//        if (cantidadRequerida <= 0) {
//            Swal.fire({
//                icon: 'warning',
//                title: 'Atención',
//                text: 'Ingresa primero la cantidad de unidades a vender.',
//                timer: 5000,
//                showConfirmButton: false
//            });
//            return;
//        }

//        let stockAcumulado = 0;
//        document.querySelectorAll('.btn-lote.active').forEach(btn => {
//            stockAcumulado += parseInt(btn.dataset.stock || 0, 10);
//        });

//        if (stockAcumulado >= cantidadRequerida) {
//            Swal.fire({
//                icon: 'info',
//                title: 'Cantidad cubierta',
//                text: `La cantidad requerida (${cantidadRequerida} und) ya está totalmente cubierta por los lotes seleccionados.`,
//                timer: 5000,
//                showConfirmButton: false
//            });
//            return;
//        }

//        lotesSeleccionados.push(loteId);
//        elementoHtml.classList.remove('btn-outline-primary');
//        elementoHtml.classList.add('btn-primary', 'active');
//    }

//    ActualizarCalculoUnidades();
//}

///**
// * 4. Recalcula la distribución respetando el orden en que se hizo clic
// */
//function CalcularDistribucionLotes(cantidadRequerida) {
//    if (!Array.isArray(lotesActuales) || lotesActuales.length === 0 || lotesSeleccionados.length === 0) {
//        return { distribucion: [], unidadesPendientes: cantidadRequerida };
//    }

//    let unidadesPendientes = cantidadRequerida;
//    const distribucion = [];

//    for (const idSeleccionado of lotesSeleccionados) {
//        const lote = lotesActuales.find(l => String(l.loteId ?? l.id) === String(idSeleccionado));

//        if (!lote) continue;

//        const loteId = lote.loteId ?? lote.id;
//        const stockDisponible = lote.cantRestante ?? lote.stock ?? 0;

//        const tomar = unidadesPendientes > 0 ? Math.min(stockDisponible, unidadesPendientes) : 0;
//        unidadesPendientes -= tomar;

//        distribucion.push({
//            loteId: loteId,
//            codigo: loteId,
//            tomadas: tomar
//        });
//    }

//    return { distribucion, unidadesPendientes };
//}

///**
// * 5. Muestra visualmente la asignación de cada lote
// */
//function ActualizarCalculoUnidades() {
//    const inputCantidad = document.getElementById('validarCantidadVendidos');
//    let cantidadAVender = parseInt(inputCantidad?.value, 10);

//    if (isNaN(cantidadAVender) || cantidadAVender <= 0) {
//        cantidadAVender = 0;
//    }

//    const resumen = document.getElementById('resumenSeleccionLotes');
//    if (!resumen) return;

//    if (!lotesActuales || lotesActuales.length === 0) {
//        return;
//    }

//    const stockTotalGlobal = lotesActuales.reduce((suma, lote) => {
//        return suma + (lote.cantRestante ?? lote.stock ?? 0);
//    }, 0);

//    if (cantidadAVender > stockTotalGlobal) {
//        cantidadAVender = stockTotalGlobal;
//        if (inputCantidad) {
//            inputCantidad.value = stockTotalGlobal;
//        }
//    }

//    if (lotesSeleccionados.length === 0) {
//        resumen.innerHTML = '<span class="text-warning fs-7">⚠️ Debe seleccionar al menos un lote para procesar la venta.</span>';
//        return;
//    }

//    if (cantidadAVender === 0) {
//        resumen.innerHTML = '<span class="text-secondary fs-7">Ingrese una cantidad válida mayor a 0.</span>';
//        return;
//    }

//    const resultado = CalcularDistribucionLotes(cantidadAVender);

//    let htmlResumen = '<div class="d-flex flex-wrap align-items-center gap-1 mb-1"><span class="fw-bold me-1 text-dark fs-7">Asignación:</span>';

//    resultado.distribucion.forEach(item => {
//        const badgeColor = item.tomadas > 0 ? 'bg-light text-primary border-primary' : 'bg-light text-muted border-secondary';

//        htmlResumen += `
//            <span class="badge ${badgeColor} border fs-7 me-1">
//                Lote ${item.codigo}: <strong class="text-dark">${item.tomadas} und</strong>
//            </span>
//        `;
//    });

//    htmlResumen += '</div>';

//    if (resultado.unidadesPendientes > 0) {
//        htmlResumen += `<span class="text-danger fw-bold fs-7">⚠️ Faltan ${resultado.unidadesPendientes} unidades por cubrir. Seleccione más lotes.</span>`;
//    } else {
//        htmlResumen += `<span class="text-success fw-bold fs-7">✔ Cantidad requerida (${cantidadAVender} und) cubierta correctamente.</span>`;
//    }

//    resumen.innerHTML = htmlResumen;
//}

///**
// * 6. Búsqueda y carga del producto
// */
//async function CargarProductoExisteParaVenta(limpiarCampos) {
//    let codigo = document.getElementById("validarCodigo")?.value;
//    let productoEncontrado = false;

//    if (limpiarCampos == true) {
//        LimpiarCamposVenta();
//    }

//    if (!codigo) {
//        console.log("El campo de código está vacío.");
//        return;
//    }

//    try {
//        const response = await fetch(`/Funciones/CargarProductoExisteParaVenta?filtroId=${codigo}`);

//        if (response.ok) {
//            const producto = await response.json();
//            productoCargadoActual = producto;

//            if (producto && Object.keys(producto).length > 0) {
//                productoEncontrado = true;

//                if (lblCantStockActual) {
//                    lblCantStockActual.setAttribute("data-valor-interno", producto.cantStock);
//                    lblCantStockActual.innerText = `${producto.cantStock} unidades en Stock`;
//                }

//                if (visualizaMarcaProducto) visualizaMarcaProducto.innerText = producto.nombreMarca || 'N/A';
//                if (visualizaColorProducto) visualizaColorProducto.innerText = producto.nombreColor || 'N/A';
//                if (visualizaNombreProducto) visualizaNombreProducto.innerText = producto.nombreProducto || 'N/A';
//                if (visualizaReferenciaProducto) visualizaReferenciaProducto.innerText = producto.referencia || 'N/A';

//                CargarLotesProductoVenta(producto);
//                CargarImagenBase64(previewImagenProducto, producto.imagenProducto);

//                if (ImagenProducto) ImagenProducto.value = producto.imagenProducto;
//            } else {
//                console.log("Producto no encontrado o datos vacíos.");
//            }
//        } else {
//            console.log("No se encontró en la respuesta de la API el producto: ", codigo);
//        }
//    } catch (error) {
//        console.error('Error al buscar producto:', error);
//    }

//    if (previewImagenProducto) {
//        if (productoEncontrado && ImagenProducto?.value) {
//            previewImagenProducto.style.display = 'block';
//            if (iconoPreviewImagenProducto) iconoPreviewImagenProducto.style.display = 'none';
//        } else {
//            previewImagenProducto.style.display = 'none';
//            if (iconoPreviewImagenProducto) iconoPreviewImagenProducto.style.display = 'block';
//        }
//    }
//}

///**
// * 7. Limpieza de interfaz
// */
//function LimpiarCamposVenta() {
//    lotesSeleccionados = [];
//    lotesActuales = null;
//    productoCargadoActual = null;

//    if (lblCantStockActual) lblCantStockActual.setAttribute("data-valor-interno", '');
//    if (inputPrecioVentaPorUni) inputPrecioVentaPorUni.value = '';
//    if (inputPrecioVentaTotal) inputPrecioVentaTotal.value = '';
//    if (lblCantStockActual) lblCantStockActual.textContent = '';
//    if (cantUnidadesVender) cantUnidadesVender.value = '';
//    if (chkManualTotal) chkManualTotal.checked = false;
//    if (chkConsumidorFinal) chkConsumidorFinal.checked = false;
//    if (precioVentaBaseOriginal) precioVentaBaseOriginal = 0;

//    if (visualizaMarcaProducto) visualizaMarcaProducto.innerText = '';
//    if (visualizaNombreProducto) visualizaNombreProducto.innerText = '';
//    if (visualizaReferenciaProducto) visualizaReferenciaProducto.innerText = '';
//    if (visualizaColorProducto) visualizaColorProducto.innerText = '';

//    if (iconoPreviewImagenProducto) iconoPreviewImagenProducto.style.display = 'block';

//    if (previewImagenProducto) {
//        previewImagenProducto.style.display = 'none';
//        previewImagenProducto.value = null;
//    }
//    if (ImagenProducto) ImagenProducto.value = null;

//    const resumen = document.getElementById('resumenSeleccionLotes');
//    if (resumen) resumen.innerHTML = '';

//    LimpiarLotesCompletamente();
//    MostrarBloqueDatosProducto(false);
//}

//function MostrarBloqueDatosProducto(mostrar) {
//    const contenedor = document.getElementById('contenedorDetallesProducto');
//    if (mostrar) {
//        if (contenedor) {
//            contenedor.classList.remove('d-none');
//            contenedor.removeAttribute('aria-hidden');
//        }
//    } else {
//        if (contenedor) {
//            contenedor.classList.add('d-none');
//            contenedor.setAttribute('aria-hidden', 'true');
//        }
//    }
//}

//const limpiarNumero = (val) => parseFloat((val || '').toString().replace(/\D/g, '')) || 0;

//if (chkManualTotal) {
//    chkManualTotal.addEventListener('change', function () {
//        if (this.checked) {
//            inputPrecioVentaTotal.removeAttribute('readonly');
//            inputPrecioVentaTotal.classList.add('bg-white');
//            inputPrecioVentaTotal.focus();

//            if (inputPrecioVentaPorUni) {
//                inputPrecioVentaPorUni.readOnly = true;
//                inputPrecioVentaPorUni.classList.add('bg-light');
//            }
//            CalcularUnidadDesdeTotal();
//        } else {
//            inputPrecioVentaTotal.readOnly = true;
//            inputPrecioVentaTotal.classList.remove('bg-white');

//            if (inputPrecioVentaPorUni) {
//                inputPrecioVentaPorUni.readOnly = false;
//                inputPrecioVentaPorUni.classList.remove('bg-light');

//                inputPrecioVentaPorUni.value = precioVentaBaseOriginal;

//                if (typeof formatoMoneda === 'function') {
//                    formatoMoneda(inputPrecioVentaPorUni, 'resPrecioVentaPorUni');
//                }

//                CalcularVenta();
//            }

//            if (typeof ActualizarCalculoUnidades === 'function') {
//                ActualizarCalculoUnidades();
//            }
//        }
//    });
//}

//if (chkConsumidorFinal) {
//    chkConsumidorFinal.addEventListener('change', function () {
//        const esConsumidorFinal = this.checked;

//        const inputsCliente = [
//            txtDocumento,
//            txtPrimerNombre,
//            txtSegundoNombre,
//            txtPrimerApellido,
//            txtSegundoApellido,
//            txtTelefono,
//            txtCorreo,
//            selectTipoVehiculo,
//            txtVehiculoPlaca,
//            txtVehiculoModelo,
//            txtVehiculoKm
//        ];

//        if (esConsumidorFinal) {
//            selectTipoDoc_antes = selectTipoDoc?.value;
//            txtDocumento_antes = txtDocumento?.value;
//            txtPrimerNombre_antes = txtPrimerNombre?.value;
//            txtSegundoNombre_antes = txtSegundoNombre?.value;
//            txtPrimerApellido_antes = txtPrimerApellido?.value;
//            txtSegundoApellido_antes = txtSegundoApellido?.value;
//            txtTelefono_antes = txtTelefono?.value;
//            txtCorreo_antes = txtCorreo?.value;
//            txtVehiculoPlaca_antes = txtVehiculoPlaca?.value;
//            selectTipoVehiculo_antes = selectTipoVehiculo?.value;
//            txtVehiculoModelo_antes = txtVehiculoModelo?.value;
//            txtVehiculoKm_antes = txtVehiculoKm?.value;

//            if (selectTipoDoc) selectTipoDoc.value = '1';
//            if (txtDocumento) txtDocumento.value = '222222222222';
//            if (txtPrimerNombre) txtPrimerNombre.value = 'Clientes';
//            if (txtSegundoNombre) txtSegundoNombre.value = '';
//            if (txtPrimerApellido) txtPrimerApellido.value = 'Varios';
//            if (txtSegundoApellido) txtSegundoApellido.value = '';
//            if (txtTelefono) txtTelefono.value = '0000000000';
//            if (txtCorreo) txtCorreo.value = '';
//            if (selectTipoVehiculo) selectTipoVehiculo.value = '';
//            if (txtVehiculoPlaca) txtVehiculoPlaca.value = '';
//            if (txtVehiculoModelo) txtVehiculoModelo.value = '';
//            if (txtVehiculoKm) txtVehiculoKm.value = '';

//            inputsCliente.forEach(input => {
//                if (input) input.readOnly = true;
//            });

//            if (selectTipoDoc) {
//                selectTipoDoc.style.pointerEvents = 'none';
//                selectTipoDoc.style.backgroundColor = '#e9ecef';
//                selectTipoDoc.tabIndex = -1;
//            }

//            if (selectTipoVehiculo) {
//                selectTipoVehiculo.style.pointerEvents = 'none';
//                selectTipoVehiculo.style.backgroundColor = '#e9ecef';
//                selectTipoVehiculo.tabIndex = -1;
//            }

//            if (btnBuscar) btnBuscar.disabled = true;

//        } else {
//            LimpiarCamposCliente();
//            if (selectTipoDoc) selectTipoDoc.value = selectTipoDoc_antes;
//            if (txtDocumento) txtDocumento.value = txtDocumento_antes;
//            if (txtPrimerNombre) txtPrimerNombre.value = txtPrimerNombre_antes;
//            if (txtSegundoNombre) txtSegundoNombre.value = txtSegundoNombre_antes;
//            if (txtPrimerApellido) txtPrimerApellido.value = txtPrimerApellido_antes;
//            if (txtSegundoApellido) txtSegundoApellido.value = txtSegundoApellido_antes;
//            if (txtTelefono) txtTelefono.value = txtTelefono_antes;
//            if (txtCorreo) txtCorreo.value = txtCorreo_antes;
//            if (txtVehiculoPlaca) txtVehiculoPlaca.value = txtVehiculoPlaca_antes;
//            if (selectTipoVehiculo) selectTipoVehiculo.value = selectTipoVehiculo_antes;
//            if (txtVehiculoModelo) txtVehiculoModelo.value = txtVehiculoModelo_antes;
//            if (txtVehiculoKm) txtVehiculoKm.value = txtVehiculoKm_antes;
//        }
//    });
//}

//function LimpiarCamposCliente() {
//    const idsInputs = [
//        'txtClienteDocumento',
//        'txtPrimerNombre',
//        'txtSegundoNombre',
//        'txtPrimerApellido',
//        'txtSegundoApellido',
//        'txtClienteTelefono',
//        'txtCorreo',
//        'txtVehiculoPlaca',
//        'txtVehiculoModelo',
//        'txtVehiculoKm'
//    ];

//    // CORREGIDO: Se usa la variable global chkConsumidorFinal
//    if (chkConsumidorFinal) chkConsumidorFinal.checked = false;

//    idsInputs.forEach(id => {
//        const input = document.getElementById(id);
//        if (input) {
//            input.value = '';
//            input.readOnly = false;
//        }
//    });

//    if (selectTipoDoc) {
//        selectTipoDoc.value = '';
//        selectTipoDoc.style.pointerEvents = '';
//        selectTipoDoc.style.backgroundColor = '';
//        selectTipoDoc.removeAttribute('tabindex');
//    }

//    if (selectTipoVehiculo) {
//        selectTipoVehiculo.value = '';
//        selectTipoVehiculo.style.pointerEvents = '';
//        selectTipoVehiculo.style.backgroundColor = '';
//        selectTipoVehiculo.removeAttribute('tabindex');
//    }

//    if (btnBuscar) btnBuscar.disabled = false;
//}

//function CalcularUnidadDesdeTotal() {
//    const total = limpiarNumero(inputPrecioVentaTotal.value);
//    const cantidad = parseInt(cantUnidadesVender.value, 10) || 0;

//    if (cantidad > 0) {
//        const precioUnidad = total / cantidad;
//        inputPrecioVentaPorUni.value = SoloFormatoMoneda(precioUnidad);
//    } else {
//        inputPrecioVentaPorUni.value = SoloFormatoMoneda(0);
//    }

//    inputPrecioVentaTotal.value = SoloFormatoMoneda(total);
//}

//if (inputPrecioVentaTotal) {
//    inputPrecioVentaTotal.addEventListener('input', function () {
//        if (chkManualTotal?.checked) {
//            CalcularUnidadDesdeTotal();
//        }
//    });

//    inputPrecioVentaTotal.addEventListener('blur', function () {
//        if (chkManualTotal?.checked) {
//            const num = limpiarNumero(this.value);
//            this.value = SoloFormatoMoneda(num);
//        }
//    });
//}

//if (cantUnidadesVender) {
//    cantUnidadesVender.addEventListener('input', function () {
//        if (chkManualTotal?.checked) {
//            CalcularUnidadDesdeTotal();
//        } else if (typeof ActualizarCalculoUnidades === 'function') {
//            ActualizarCalculoUnidades();
//        }
//    });
//}

//function CalcularVenta() {
//    const stockActual = parseInt(lblCantStockActual?.getAttribute("data-valor-interno") || 0);
//    const cantUniVender = parseFloat(cantUnidadesVender?.value) || 0;
//    const precio = parseCurrency(inputPrecioVentaPorUni?.value);

//    let total = 0;

//    if (cantUniVender > stockActual) {
//        total = stockActual * precio;
//    } else {
//        total = cantUniVender * precio;
//    }

//    if (inputPrecioVentaPorUni) {
//        inputPrecioVentaPorUni.value = SoloFormatoMoneda(precio);
//    }

//    if (inputPrecioVentaTotal) {
//        inputPrecioVentaTotal.value = SoloFormatoMoneda(total);
//    }
//}

//function LimpiarLotesCompletamente() {
//    lotesSeleccionados = [];

//    const contenedorLotes = document.getElementById('contenedorLotes');

//    if (contenedorLotes) {
//        contenedorLotes.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
//            const instance = bootstrap.Tooltip.getInstance(el);
//            if (instance) instance.dispose();
//        });

//        contenedorLotes.innerHTML = '';
//    }

//    const contenedorAsignacion = document.getElementById('contenedorAsignacion');
//    if (contenedorAsignacion) {
//        contenedorAsignacion.innerHTML = '';
//    }

//    if (typeof ActualizarCalculoUnidades === 'function') {
//        ActualizarCalculoUnidades();
//    }
//}

//async function AgregarProductoAlCarrito(productoActual) {
//    const yaExiste = carritoVenta.some(item => String(item.codigoProducto) === String(productoActual.codigoProducto));

//    if (yaExiste) {
//        MostrarAlerta("warning", "Atención", "Este producto ya fue agregado a la lista de venta.", 4000);
//        return;
//    }

//    const cantidad = parseInt(cantUnidadesVender.value, 10) || 0;
//    const precioUnid = limpiarNumero(inputPrecioVentaPorUni.value);
//    const total = cantidad * precioUnid;

//    if (cantidad <= 0 || precioUnid <= 0) {
//        MostrarAlerta("warning", "Atención", "Ingrese una cantidad y precio válidos.", 4000);
//        return;
//    }

//    if (!lotesSeleccionados || lotesSeleccionados.length === 0) {
//        MostrarAlerta("warning", "Atención", "Debe seleccionar al menos un lote para este producto.", 4000);
//        return;
//    }

//    const distribucionResult = CalcularDistribucionLotes(cantidad);

//    if (distribucionResult.unidadesPendientes > 0) {
//        MostrarAlerta("warning", "Atención", `Aún faltan ${distribucionResult.unidadesPendientes} unidades por cubrir. Seleccione más lotes.`, 10000);
//        return;
//    }

//    const lotesEstandarizados = distribucionResult.distribucion
//        .filter(item => item.tomadas > 0)
//        .map(item => {
//            const loteObj = lotesActuales.find(l => String(l.loteId ?? l.loteId ?? l.id) === String(item.loteId));
//            const id = item.loteId;

//            let refLote = loteObj?.referencia || loteObj?.Referencia || loteObj?.nombreLote || loteObj?.codigoLote || loteObj?.numLote;

//            if (!refLote || refLote.toString().trim().toUpperCase() === 'NO APLICA' || refLote.toString().trim() === '') {
//                refLote = `Lote ${id}`;
//            }

//            return {
//                loteId: id,
//                referencia: refLote,
//                cantidad: item.tomadas
//            };
//        });

//    let nomProducto = (productoActual.nombreProducto || '').toUpperCase().trim();
//    let marcaProducto = (productoActual.nombreMarca || '').toUpperCase().trim();
//    let colorProducto = (productoActual.nombreColor || '').toUpperCase().trim();
//    let referenciaProducto = (productoActual.referencia || '').toUpperCase().trim();

//    let detalleProducto = [nomProducto, marcaProducto, colorProducto, referenciaProducto]
//        .filter(val => val !== '' && val !== 'NO APLICA')
//        .join(' - ');

//    const detalleItem = {
//        codigoProducto: productoActual.codigoProducto,
//        nombreProducto: detalleProducto,
//        cantidad: cantidad,
//        precioUnitario: precioUnid,
//        total: total,
//        lotes: lotesEstandarizados
//    };

//    // 1. Obtener/Crear Consecutivo de Pedido
//    const idPedido = await CargarConsecutivoPedido();
//    if (!idPedido) {
//        MostrarAlerta("warning", "Error de Pedido", "No se pudo obtener ni generar el consecutivo del pedido.", 5000);
//        return;
//    }

//    // 2. Solo agregar al carrito en memoria (sin afectar BD todavía)
//    carritoVenta.push(detalleItem);
//    RenderizarTablaVenta();
//    LimpiarCamposVenta();
//    if (validarCodigo) validarCodigo.value = '';
//}

//function RenderizarTablaVenta() {
//    const tbody = document.getElementById('tbodyDetalleVenta');
//    const lblTotalPagar = document.getElementById('lblTotalPagar');

//    if (!tbody) return;

//    if (carritoVenta.length === 0) {
//        tbody.innerHTML = `
//            <tr id="trFilaVacia">
//                <td colspan="6" class="text-center text-muted py-4">No hay productos agregados a la venta.</td>
//            </tr>`;
//        if (lblTotalPagar) lblTotalPagar.value = SoloFormatoMoneda(0);
//        return;
//    }

//    let html = '';
//    let granTotal = 0;

//    carritoVenta.forEach((item, index) => {
//        granTotal += item.total;

//        const textoLotes = (item.lotes && item.lotes.length > 0)
//            ? item.lotes.map(l => `${l.referencia} (${l.cantidad} und)`).join(', ')
//            : 'Sin lote asignado';

//        html += `
//            <tr>
//                <td class="text-center fw-bold fs-7">${item.codigoProducto}</td>
//                <td>
//                    <div class="fw-bold">${item.nombreProducto}</div>
//                    <small class="text-muted fs-8 d-block">
//                        Lotes: ${textoLotes}
//                    </small>
//                </td>
//                <td class="text-center fw-bold">${item.cantidad}</td>
//                <td class="text-end">${SoloFormatoMoneda(item.precioUnitario)}</td>
//                <td class="text-end fw-bold text-success">${SoloFormatoMoneda(item.total)}</td>
//                <td class="text-center">
//                    <button class="btn btn-outline-danger btn-sm border-0" onclick="EliminarItemCarrito(${index})">
//                        <i class="fa-solid fa-trash-arrow-up"></i> Quitar
//                    </button>
//                </td>
//            </tr>`;
//    });

//    tbody.innerHTML = html;
//    if (lblTotalPagar) lblTotalPagar.value = SoloFormatoMoneda(granTotal);
//}

//function EliminarItemCarrito(index) {
//    carritoVenta.splice(index, 1);
//    RenderizarTablaVenta();
//    if (typeof calcularTotalPagado_Y_Restante === 'function') {
//        calcularTotalPagado_Y_Restante(null, null, null);
//    }
//}

//function alCargarProductoExitoso(producto) {
//    productoCargadoActual = producto;
//    if (visualizaMarcaProducto) visualizaMarcaProducto.textContent = producto.marca;
//    if (visualizaNombreProducto) visualizaNombreProducto.textContent = producto.nombre;
//}

//function prepararYAgregarProducto() {
//    if (!productoCargadoActual) {
//        MostrarAlerta("warning", "Atención", "Primero debe buscar y seleccionar un producto.", 4000);
//        return;
//    }

//    AgregarProductoAlCarrito(productoCargadoActual);
//    if (typeof calcularTotalPagado_Y_Restante === 'function') {
//        calcularTotalPagado_Y_Restante(null, null, null);
//    }

//}

//async function BuscarCliente() {
//    let clienteEncontrado = false;

//    // 1. Aseguramos conversión a String y eliminamos espacios en blanco
//    const numeroDocumento = String(txtDocumento?.value || '').trim();

//    // 2. Validamos la longitud real del texto
//    if (!numeroDocumento || numeroDocumento.length < 6) {
//        MostrarAlerta("warning", "Documento incompleto", "Agregue un número de documento válido.", 10000);
//        return;
//    }

//    try {
//        const response = await fetch(`/Usuarios/ObtenerUsuario?filtroId=${encodeURIComponent(numeroDocumento)}`);

//        if (response.ok) {
//            const clienteActual = await response.json();

//            // 3. Manejo seguro para determinar si viene un Objeto o un Array con datos
//            const esValido = clienteActual && (
//                Array.isArray(clienteActual)
//                    ? clienteActual.length > 0
//                    : Object.keys(clienteActual).length > 0
//            );

//            if (esValido) {
//                // Si la respuesta es un Array, se toma el primer registro
//                const cliente = Array.isArray(clienteActual) ? clienteActual[0] : clienteActual;

//                clienteEncontrado = true;

//                // Mapeo con soporte para camelCase y PascalCase
//                if (selectTipoDoc) selectTipoDoc.value = cliente.tipoDocumentos.tipoDocumentoId ?? cliente.tipoDocumentos.tipoDocumentoId ?? '';
//                if (txtDocumento) txtDocumento.value = cliente.documento ?? cliente.documento ?? '';
//                if (txtPrimerNombre) txtPrimerNombre.value = cliente.primerNombre ?? cliente.primerNombre ?? '';
//                if (txtSegundoNombre) txtSegundoNombre.value = cliente.segundoNombre ?? cliente.segundoNombre ?? '';
//                if (txtPrimerApellido) txtPrimerApellido.value = cliente.primerApellido ?? cliente.primerApellido ?? '';
//                if (txtSegundoApellido) txtSegundoApellido.value = cliente.segundoApellido ?? cliente.segundoApellido ?? '';
//                if (txtTelefono) txtTelefono.value = cliente.telefonoMovil ?? cliente.telefonoMovil ?? '';
//                if (txtCorreo) txtCorreo.value = cliente.correo ?? cliente.correo ?? '';
//            } else {
//                MostrarAlerta("warning", "Cliente", "Datos vacíos del usuario.", 10000);
//            }
//        } else {
//            MostrarAlerta("warning", "Cliente", "Usuario no encontrado.", 10000);
//        }
//    } catch (error) {
//        console.error('Error al buscar cliente:', error);
//        MostrarAlerta("warning", "Cliente", "Error al buscar usuario: " + error, 10000);
//    }
//}

//async function RegistrarVentaProducto() {
//    var tipoDocumentoVenta = selectTipoDoc?.value;
//    var documentoVenta = txtDocumento?.value?.trim();
//    var primerNombreVenta = txtPrimerNombre?.value?.trim();
//    var primerApellidoVenta = txtPrimerApellido?.value?.trim();

//    // Validaciones de cliente
//    if (!tipoDocumentoVenta) {
//        selectTipoDoc.focus();
//        MostrarAlerta("warning", "Datos cliente", "Por favor ingresa el tipo de documento del cliente.", 10000);
//        return;
//    }
//    if (!documentoVenta) {
//        txtDocumento.focus();
//        txtDocumento.select();
//        MostrarAlerta("warning", "Datos cliente", "Por favor ingresa el número de documento del cliente.", 10000);
//        return;
//    }
//    if (!primerNombreVenta) {
//        txtPrimerNombre.focus();
//        txtPrimerNombre.select();
//        MostrarAlerta("warning", "Datos cliente", "Por favor ingresa el primer nombre del cliente.", 10000);
//        return;
//    }
//    if (!primerApellidoVenta) {
//        txtPrimerApellido.focus();
//        txtPrimerApellido.select();
//        MostrarAlerta("warning", "Datos cliente", "Por favor ingresa el primer apellido del cliente.", 10000);
//        return;
//    }
//    if (carritoVenta.length === 0) {
//        if (validarCodigo) {
//            validarCodigo.focus();
//            validarCodigo.select();
//        }
//        MostrarAlerta("warning", "Datos Producto", "Por favor ingresa un producto a la venta.", 10000);
//        return;
//    }
//    if (!ValidarMetodosPago()) {
//        return;
//    }

//    const listaPagos = ObtenerMetodosPagoAgregados();
//    if (listaPagos.length === 0) {
//        MostrarAlerta("warning", "Método de Pago", "No se encontraron datos en los métodos de pago.", 10000);
//        return;
//    }

//    // A. Procesar productos en la BD (Inserción en Salida, Lotes, Stock y Ganancias)
//    for (const item of carritoVenta) {
//        const ok = await IniciarProcesoProducto(item);
//        if (!ok) {
//            // IniciarProcesoProducto ya incluye su propio rollback interno si falla un producto
//            return;
//        }
//    }

//    // B. Registrar Maestro de Venta / Cliente / Vehículo
//    const objetoDatosFactura = {
//        TallerId: parseInt(tallerId, 10),
//        ConsecutivoPedido: ConsecutivoActual,
//        Cliente: {
//            TipoDocumentoId: parseInt(tipoDocumentoVenta, 10),
//            Documento: documentoVenta,
//            PrimerNombre: primerNombreVenta,
//            SegundoNombre: txtSegundoNombre?.value?.trim() || '',
//            PrimerApellido: primerApellidoVenta,
//            SegundoApellido: txtSegundoApellido?.value?.trim() || '',
//            Telefono: txtTelefono?.value?.trim() || '',
//            Correo: txtCorreo?.value?.trim() || '',
//            EsConsumidorFinal: chkConsumidorFinal?.checked || false
//        },
//        Vehiculo: {
//            TipoVehiculoId: selectTipoVehiculo?.value ? parseInt(selectTipoVehiculo.value, 10) : null,
//            Placa: txtVehiculoPlaca?.value?.trim() || '',
//            Modelo: txtVehiculoModelo?.value?.trim() || '',
//            Kilometraje: txtVehiculoKm?.value ? parseInt(txtVehiculoKm.value, 10) : 0
//        },
//        DetalleObservacion: validarDetalle?.value?.trim() || '',
//        TotalPagar: limpiarNumero(document.getElementById('lblTotalPagar')?.value)
//    };

//    try {
//        const responseVenta = await fetch(`/Ventas/RegistrarVentaMaestro?filtroId=${tallerId}`, {
//            method: 'POST',
//            headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
//            body: JSON.stringify(objetoDatosFactura)
//        });

//        if (!responseVenta.ok) {
//            MostrarAlerta("error", "Error Venta", "No se pudo registrar la venta en la base de datos.", 10000);
//            return;
//        }

//        // C. Registrar métodos de pago UNO A UNO
//        const pagosExitosos = await RegistrarMetodosPagoUnoPorUno(listaPagos, ConsecutivoActual);

//        if (pagosExitosos) {
//            MostrarAlerta("success", "Venta Registrada", `Se registró la venta con factura / pedido #: ${ConsecutivoActual}`, 10000);

//            LimpiarCamposCliente();
//            LimpiarCamposVenta();
//            LimpiarFormularioVenta();
//        } else {
//            MostrarAlerta("warning", "Atención", "Venta procesada, pero ocurrió un problema guardando algunos métodos de pago.", 10000);
//        }

//    } catch (error) {
//        console.error("Error al finalizar el registro de la venta:", error);
//        MostrarAlerta("error", "Error Crítico", "Ocurrió una falla inesperada en el servidor.", 10000);
//    }
//}



//function ObtenerMetodosPagoAgregados() {
//    const contenedorPagos = document.getElementById('paymentMethodsContainer');
//    const metodosPago = [];

//    if (!contenedorPagos) return metodosPago;

//    const filasPago = contenedorPagos.querySelectorAll('.payment-method-row, .row');

//    filasPago.forEach(fila => {
//        const selectMetodo = fila.querySelector('select[name="MetodoPagoId"], select.select-metodo-pago, select');
//        const inputMonto = fila.querySelector('input[name="MontoPago"], input.input-monto-pago, input[type="text"]');
//        const inputReferencia = fila.querySelector('input[name="ReferenciaPago"], input.input-referencia-pago');

//        if (selectMetodo && inputMonto) {
//            const metodoPagoId = parseInt(selectMetodo.value, 10);
//            const valorPagado = limpiarNumero(inputMonto.value);
//            const referencia = inputReferencia ? inputReferencia.value.trim() : '';

//            if (metodoPagoId > 0 && valorPagado > 0) {
//                metodosPago.push({
//                    MetodoPagoId: metodoPagoId,
//                    ValorPagado: valorPagado,
//                    ReferenciaPago: referencia
//                });
//            }
//        }
//    });

//    return metodosPago;
//}

///**
// * 2. REGISTRAR MÉTODOS DE PAGO: Inserción uno a uno en la BD
// */
//async function RegistrarMetodosPagoUnoPorUno(listaPagos, consecutivoPedido) {
//    for (const pago of listaPagos) {
//        const payloadPago = {
//            TallerId: parseInt(tallerId, 10),
//            ConsecutivoPedido: consecutivoPedido,
//            MetodoPagoId: pago.MetodoPagoId,
//            ValorPagado: pago.ValorPagado,
//            ReferenciaPago: pago.ReferenciaPago
//        };

//        try {
//            const response = await fetch(`/VentasPagos/AgregarPagoVenta?filtroId=${tallerId}`, {
//                method: 'POST',
//                headers: {
//                    'Content-Type': 'application/json',
//                    'Accept': 'application/json'
//                },
//                body: JSON.stringify(payloadPago)
//            });

//            if (!response.ok) {
//                const errorText = await response.text();
//                console.error(`Error al registrar pago ${pago.MetodoPagoId}:`, errorText);
//                return false;
//            }
//        } catch (error) {
//            console.error("Error de red al registrar método de pago:", error);
//            return false;
//        }
//    }
//    return true;
//}

//function ValidarMetodosPago() {
//    const contenedorPagos = document.getElementById('paymentMethodsContainer');
//    const cantidadPagos = contenedorPagos ? contenedorPagos.children.length : 0;

//    if (cantidadPagos === 0) {
//        MostrarAlerta("warning", "Método de Pago", "Debe agregar al menos un método de pago para completar la venta.", 10000);

//        const btnAgregarPago = document.getElementById('addPaymentMethod');
//        if (btnAgregarPago) {
//            btnAgregarPago.focus();
//        }
//        return false;
//    }
//    return true;
//}

//async function CargarConsecutivoPedido() {
//    if (!tallerId || tallerId <= 0) {
//        console.warn("El ID del taller no es válido.");
//        return null;
//    }

//    // Si ya existe en memoria, se retorna inmediatamente
//    if (ConsecutivoActual) {
//        return ConsecutivoActual;
//    }

//    try {
//        const response = await fetch(`/Pedidos/CrearPedido?filtroId=${tallerId}`, {
//            method: 'POST',
//            headers: {
//                'Content-Type': 'application/json'
//            }
//        });

//        if (response.ok) {
//            const pedido = await response.json();

//            if (pedido && pedido.consecutivoPedidoCreado) {
//                //IdConsecutivoActual = pedido.pedidoId;
//                ConsecutivoActual = pedido.consecutivoPedidoCreado;

//                if (lblNumeroPedido) {
//                    lblNumeroPedido.textContent = ConsecutivoActual;
//                }
//                return ConsecutivoActual;

//            }
//        } else {
//            console.error("Error en la respuesta del servidor:", response.statusText);
//        }
//    } catch (error) {
//        console.error("Error al conectar con la API de Pedidos:", error);
//    }
//    return null;
//}
//function LimpiarFormularioVenta() {
//    // Limpiar productos
//    const tbody = document.getElementById('tbodyDetalleVenta');
//    const lblTotalPagar = document.getElementById('lblTotalPagar');

//    if (carritoVenta.length > 0) {
//        tbody.innerHTML = `
//            <tr id="trFilaVacia">
//                <td colspan="6" class="text-center text-muted py-4">No hay productos agregados a la venta.</td>
//            </tr>`;
//        if (lblTotalPagar) lblTotalPagar.value = SoloFormatoMoneda(0);
//        if (validarPrecioTotalPagado) validarPrecioTotalPagado.value = SoloFormatoMoneda(0);
//        if (validarPrecioRestantePorPagar) validarPrecioRestantePorPagar.value = SoloFormatoMoneda(0);
//    }

//    //Limpiar detalle
//    if (validarDetalle) {
//        validarDetalle.value = "";
//    }

//    //Limpiar pagos
//    const contenedorPagos = document.getElementById('paymentMethodsContainer');
//    const cantidadPagos = contenedorPagos ? contenedorPagos.children.length : 0;
//    if (cantidadPagos > 0) {
//        if (contenedorPagos) {
//            contenedorPagos.innerHTML = "";
//        }
//    }

//    carritoVenta = [];
//    ConsecutivoActual = null;
//    //IdConsecutivoActual = null;
//    lblNumeroPedido.textContent = null;

//}


//async function IniciarProcesoProducto(productoData) {
//    if (!tallerId || tallerId <= 0) {
//        console.warn("Taller no válido.");
//        return false;
//    }

//    if (!ConsecutivoActual) {
//        console.warn("Pedido no válido.");
//        return false;
//    }

//    if (!productoData?.lotes?.length) {
//        console.warn("El producto no contiene información válida de lotes.");
//        return false;
//    }

//    const productosInsertadosIds = [];
//    const lotesProcesados = [];
//    let stockActualizadoExitosamente = false;

//    // Función de rollback en BD si falla algún punto de la transacción
//    const ejecutarRollback = async () => {
//        console.warn("Iniciando proceso de rollback para revertir operaciones en BD...");

//        // 1. Revertir Stock General
//        if (stockActualizadoExitosamente) {
//            await RevertirStockVenta({
//                CantVendidos: productoData.cantidad,
//                CodigoProducto: productoData.codigoProducto,
//                TallerId: parseInt(tallerId)
//            });
//        }

//        // 2. Revertir Lotes procesados
//        for (const lote of lotesProcesados) {
//            await RevertirLoteVenta({
//                LoteId: lote.loteId,
//                CantVendidos: lote.cantidad,
//                CodigoProducto: productoData.codigoProducto,
//                TallerId: parseInt(tallerId)
//            });
//        }

//        // 3. Eliminar Productos insertados de la tabla
//        for (const idProd of productosInsertadosIds) {
//            await EliminarProductoEspecificoTabla(idProd);
//        }

//        // 4. ELIMINAR EL PEDIDO CREADO Y REINICIAR CONSECUTIVO EN MEMORIA
//        if (ConsecutivoActual) {
//            await EliminarPedido(ConsecutivoActual);
//            //IdConsecutivoActual = null;
//            ConsecutivoActual = null;
//            if (lblNumeroPedido) lblNumeroPedido.textContent = '';
//        }
//    };

//    try {
//        // 1. REGISTRAR EN TABLA, ACTUALIZAR LOTES Y GANANCIAS
//        for (const lote of productoData.lotes) {
//            const dataProducto = {
//                PrecioFinalXuni: productoData.precioUnitario,
//                CantVendidos: lote.cantidad,
//                CodigoProducto: productoData.codigoProducto,
//                TallerId: parseInt(tallerId),
//                ConsecutivoPedido: ConsecutivoActual,
//                LoteId: lote.loteId
//            };

//            // A. Registrar Producto
//            let idProductoInsertado = await AgregarProductoTabla(dataProducto);
//            if (!idProductoInsertado) {
//                MostrarAlerta("warning", "Error", `Error al registrar producto en Lote ${lote.loteId}.`, 10000);
//                await ejecutarRollback();
//                return false;
//            }
//            productosInsertadosIds.push(idProductoInsertado);

//            // B. Actualizar Lote
//            const dataLoteProducto = {
//                LoteId: lote.loteId,
//                CantVendidos: lote.cantidad,
//                CodigoProducto: productoData.codigoProducto,
//                TallerId: parseInt(tallerId)
//            };

//            let loteActualizado = await ActualizarLotesVenta(dataLoteProducto);
//            if (!loteActualizado) {
//                MostrarAlerta("warning", "Error Lote", `Error al actualizar stock del Lote ${lote.loteId}.`, 10000);
//                await ejecutarRollback();
//                return false;
//            }
//            lotesProcesados.push(lote);

//            // C. Actualizar Ganancia por Lote
//            const dataGananciaProducto = {
//                TallerId: parseInt(tallerId),
//                LoteId: lote.loteId,
//                CodigoProducto: productoData.codigoProducto
//            };

//            let gananciaActualizada = await ActualizarGananciaVenta(dataGananciaProducto);
//            if (!gananciaActualizada) {
//                MostrarAlerta("warning", "Error Ganancia", `No se pudo registrar la ganancia del Lote ${lote.loteId}.`, 10000);
//                await ejecutarRollback();
//                return false;
//            }
//        }

//        // 2. ACTUALIZAR STOCK GENERAL
//        const dataStockProducto = {
//            CantVendidos: productoData.cantidad,
//            CodigoProducto: productoData.codigoProducto,
//            TallerId: parseInt(tallerId)
//        };

//        let stockActualizado = await ActualizarStockVenta(dataStockProducto);
//        if (!stockActualizado) {
//            MostrarAlerta("warning", "Error Stock", "Error al actualizar stock general del producto.", 10000);
//            await ejecutarRollback();
//            return false;
//        }
//        stockActualizadoExitosamente = true;

//        MostrarAlerta("success", "Éxito", "Producto, lotes, stock y ganancias procesados correctamente.", 10000);
//        return true;

//    } catch (error) {
//        console.error("Error crítico durante la transacción:", error);
//        await ejecutarRollback();
//        MostrarAlerta("error", "Error Crítico", "Ocurrió una falla inesperada. Se han revertido las operaciones.", 10000);
//        return false;
//    }
//}


//async function AgregarProductoTabla(dataProducto) {
//    try {
//        const idTaller = parseInt(document.getElementById("txtTallerId")?.value, 10);

//        if (!idTaller || isNaN(idTaller)) {
//            console.error("El TallerId no es válido.");
//            return null;
//        }

//        const response = await fetch(`/InventarioSalidaProductos/AgregarProductoVenta?filtroId=${idTaller}`, {
//            method: 'PUT',
//            headers: {
//                'Content-Type': 'application/json',
//                'Accept': 'application/json'
//            },
//            body: JSON.stringify(dataProducto)
//        });

//        if (response.ok) {
//            const resultado = await response.json();
//            return resultado.id || resultado.productoId || true;
//        } else {
//            const errorMsg = await response.text();
//            console.error(`Error HTTP ${response.status} en AgregarProductoVenta:`, errorMsg);
//            return null;
//        }
//    } catch (error) {
//        console.error("Error al registrar producto en tabla:", error);
//        return null;
//    }
//}

//async function ActualizarStockVenta(dataStockProducto) {
//    try {
//        const response = await fetch(`/InventarioStocks/ActualizarStockVenta?filtroId=${tallerId}`, {
//            method: 'PUT',
//            headers: { 'Content-Type': 'application/json' },
//            body: JSON.stringify(dataStockProducto)
//        });

//        if (response.ok) {
//            const resultado = await response.json();
//            return resultado.id || resultado.stockId || true;
//        }
//        return null;
//    } catch (error) {
//        console.error("Error al actualizar stock:", error);
//        return null;
//    }
//}

//async function ActualizarLotesVenta(dataLoteProducto) {
//    try {
//        const response = await fetch(`/InventarioLotes/ActualizarLotesVenta?filtroId=${tallerId}`, {
//            method: 'PUT',
//            headers: { 'Content-Type': 'application/json' },
//            body: JSON.stringify(dataLoteProducto)
//        });

//        if (response.ok) {
//            const resultado = await response.json();
//            return resultado.id || resultado.loteId || true;
//        }
//        return null;
//    } catch (error) {
//        console.error("Error al actualizar lote:", error);
//        return null;
//    }
//}

//async function ActualizarGananciaVenta(dataGanancia) {
//    try {
//        const response = await fetch(`/InventarioGanancias/ActualizarGananciaVenta?filtroId=${tallerId}`, {
//            method: 'PUT',
//            headers: { 'Content-Type': 'application/json' },
//            body: JSON.stringify(dataGanancia)
//        });

//        if (response.ok) {
//            const resultado = await response.json();
//            return resultado.id || true;
//        }
//        return null;
//    } catch (error) {
//        console.error("Error al ejecutar SP de ganancia:", error);
//        return null;
//    }
//}


//// Revertir Lote (Suma la cantidad de vuelta)
//async function RevertirLoteVenta(dataLote) {
//    try {
//        await fetch(`/InventarioLotes/RevertirLoteVenta?filtroId=${tallerId}`, {
//            method: 'PUT',
//            headers: { 'Content-Type': 'application/json' },
//            body: JSON.stringify(dataLote)
//        });
//    } catch (e) { console.error("Error al revertir lote", e); }
//}

//// Revertir Stock (Suma la cantidad de vuelta al general)
//async function RevertirStockVenta(dataStock) {
//    try {
//        await fetch(`/InventarioStocks/RevertirStockVenta?filtroId=${tallerId}`, {
//            method: 'PUT',
//            headers: { 'Content-Type': 'application/json' },
//            body: JSON.stringify(dataStock)
//        });
//    } catch (e) { console.error("Error al revertir stock", e); }
//}

//// Eliminar fila de producto individual por ID
//async function EliminarProductoEspecificoTabla(idProducto) {
//    try {
//        await fetch(`/InventarioSalidaProductos/EliminarPorId/${idProducto}?filtroId=${tallerId}`, {
//            method: 'DELETE'
//        });
//    } catch (e) { console.error("Error al eliminar fila del producto", e); }
//}

//async function EliminarPedido(pedidoId) {
//    if (!pedidoId || !tallerId) return;

//    try {
//        await fetch(`/Pedidos/EliminarPedido/${pedidoId}?filtroId=${tallerId}`, {
//            method: 'DELETE'
//        });
//    } catch (e) {
//        console.error("Error al eliminar el pedido en el rollback:", e);
//    }
//}