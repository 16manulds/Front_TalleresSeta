// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


'use strict';

// Obtener complementos y crear un select
function TallerComplementos(padre, hijo, ruta, filtroAdicional) {
    var urlTipos = ruta;
    var ddlSource = padre;
    var ddlFiltro = filtroAdicional;
    $.getJSON(urlTipos, { filtroId: $(ddlSource).val(), filtro: $(ddlFiltro).val() }, function (data) {
        var items = '<option value="">Seleccionar...</option>';
        $(hijo).empty();
        if (data !== null) {
            $.each(data, function (i, dato) {
                items += '<option value="' + dato.value + '">' + dato.text + '</option>';
            });
            $(hijo).html(items);
        }
    });
}


'use strict';
function TallerAllProductos(padre, hijo, ruta, filtroAdicional) {
    var urlTipos = ruta;
    var ddlSource = padre;
    var ddlFiltro = filtroAdicional;
    $.getJSON(urlTipos, { filtroId: $(ddlSource).val(), filtro: $(ddlFiltro).val() }, function (data) {
        var items = '<option value="">Seleccionar...</option>';
        $(hijo).empty();
        if (data !== null) {
            //$.each(data, function (i, dato) {
            //    items += '<option value="' + dato.value + '">' + dato.text + '</option>';
            //});

            var codigosProductos = Array.from(new Set(data.map(p => p.CodigoProducto)));

            codigosProductos.forEach(function (codigoProducto) {
                //var optgroupElement = document.createElement('optgroup');
                var codigoP = codigoProducto;

                var productosFiltrados = productos.filter(p => p.CodigoProducto === codigoProducto)
                    .sort((a, b) => b.PrecioVentaUni - a.PrecioVentaUni);

                productosFiltrados.forEach(function (producto) {
                    var idProducto = producto.InventarioEntradaProductoId;
                    var nombreP = producto.NombreProducto;

                    items += '<optgroup label="' + codigoP + '">';
                    items += '<option value="' + idProducto + '">';
                    items += nombreP;
                    items += '</option >';
                    items += '<optgroup>';
                });
            });
            $(hijo).html(selectElement);
        }
    });
}

// Obtener todos los productos en stock
function TodosLosProductos(hijo, ruta) {
    var urlTipos = ruta;
    $.getJSON(urlTipos, function (data) {
        var items = '<option value="">Seleccionar...</option>';
        $(hijo).empty();
        if (data !== null) {
            $.each(data, function (i, dato) {
                items += '<option value="' + dato.value + '">' + dato.text + '</option>';
            });
            $(hijo).html(items);
        }
    });
}


// Obtener complementos y crear un select
function SionComplementos(padre, hijo, ruta, filtroAdicional) {
    var urlTipos = ruta;
    var ddlSource = padre;
    var ddlFiltro = filtroAdicional;
    $.getJSON(urlTipos, { filtroId: $(ddlSource).val(), filtro: $(ddlFiltro).val() }, function (data) {
        var items = '<option value="">Seleccionar...</option>';
        $(hijo).empty();
        if (data !== null) {
            $.each(data, function (i, dato) {
                items += '<option value="' + dato.value + '">' + dato.text + '</option>';
            });
            $(hijo).html(items);
        }
    });
}

