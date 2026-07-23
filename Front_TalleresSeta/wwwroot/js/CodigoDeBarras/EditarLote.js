document.addEventListener("DOMContentLoaded", () => {
    let ImagenProducto = document.getElementById('ImagenProducto');
    let previewImagenProducto = document.getElementById('previewImagenProducto');
    let iconoPreviewImagenProducto = document.getElementById('iconoPreviewImagenProducto');
    let cantidadIngresan = document.getElementById('validarCantidadIngresan');
    let cantidadRestante = document.getElementById('validarCantidadRestante');

    function CargarImagenBase64(canvaImagen, imagenBase) {
        if (canvaImagen.id == 'previewImagenProducto' && (imagenBase == null || imagenBase == '')) {
            iconoPreviewImagenProducto.style.display = 'block';
            previewImagenProducto.style.display = 'none';
        } else if (canvaImagen.id == 'previewImagenProducto' && imagenBase != null) {
            iconoPreviewImagenProducto.style.display = 'none';
            previewImagenProducto.style.display = 'block';
        }

        if (imagenBase && canvaImagen) {
            let existingBase64 = imagenBase; // Obtiene la cadena Base64 del input hidden
            if (existingBase64) {
                let imageUrl = `data:image/png;base64,${existingBase64}`;
                canvaImagen.style.display = 'block'; // Muestra la imagen
                canvaImagen.src = imageUrl;
            } else {
                canvaImagen.style.display = 'none'; // Asegúrate de que esté oculta si no hay imagen
                console.log("No hay imagen Base64 existente en el hidden input.");
            }
        } else {
            console.warn(`Elementos HTML 'ImagenProducto' o ' ${canvaImagen} ' no encontrados.`);
        }
    }

    function HabilitarCampoIngresan() {
        let valorUniIngresan = cantidadIngresan.value.replace(/\D/g, '');
        let valorUniRestante = cantidadRestante.value.replace(/\D/g, '');
        if (valorUniRestante > 0) {
            if (valorUniIngresan == valorUniRestante) {
                cantidadIngresan.readOnly = false;
            } else {
                cantidadIngresan.readOnly = true;
            }
        } else {
            cantidadIngresan.readOnly = false;
        }
    }

    //async function AjustarCantidadesLote(cantidadIngresan, cantidadRestante) {

    //const response = await fetch(`/Funciones/ActualCantidadIngresanPorLote?LoteId=${valorCodLote}&CodigoProducto=${valorCodProducto}`);
    //if (!response.ok) throw new Error("Error al consultar el último valor");
    //const data = await response.json(); // obtiene el valor como texto
    // el campo debe ser tal cual lo regresa el api pero iniciando en minuscula, debe ser como en la BD el sp
    //let valorActualIngresaBD = parseInt(data.cantIngresan) || 0;

    //}


    cantidadIngresan.addEventListener("input", () => {
        cantidadRestante.value = cantidadIngresan.value.replace(/\D/g, '');
        cantidadIngresan.value = cantidadIngresan.value.replace(/\D/g, '');        
    });

    window.onload = function () {
        if (ImagenProducto != null) {
            CargarImagenBase64(previewImagenProducto, ImagenProducto.value); // Cargar imagen del producto
        }

        HabilitarCampoIngresan();
    };

});