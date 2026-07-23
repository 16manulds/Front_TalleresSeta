document.addEventListener("DOMContentLoaded", () => {
    let validaPlaca = document.getElementById("validarPlaca");
    const ingresaVehiculo = document.getElementById("lblIngresaVehiculo");
    const vehiculoYaIngresado = document.getElementById("lblVehiculoYaIngresado");
        
    
    async function ValidarPlaca(placa) {
        try {
            const response = await fetch(`/Funciones/ValidarPlaca?filtroId=${placa}`);

            if (response.ok) {
                // Obtiene el valor como texto
                const data = await response.text();

                // Compara el texto 'true' directamente
                if (data === 'true') {
                    vehiculoYaIngresado.style.display = 'block';
                    ingresaVehiculo.style.display = 'none';
                    console.log("Vehículo ya fue registrado.");
                    Swal.fire({
                        icon: 'warning',
                        title: 'Vehículo ingresado',
                        text: 'El vehículo ya fue ingresado anteriormente.',
                        timer: 5000,
                        showConfirmButton: false
                    });
                } else {
                    console.log("Vehículo no encontrado o datos vacíos.");
                    ingresaVehiculo.style.display = 'block';
                    vehiculoYaIngresado.style.display = 'none';
                }
            } else if (response.status === 404) {
                console.log("vehículo no encontrado. Código de estado: 404.");
            } else {
                console.log("No se encontró en la respuesta de la API el vehículo: ", codigo);
            }
        } catch (error) {
            console.error('Error al buscar vehículo:', error);            
        }        
    }
  

    validaPlaca.addEventListener("input", () => {        
        validaPlaca.value = validaPlaca.value.toUpperCase();
        let placa = validaPlaca.value;
        ValidarPlaca(placa);
    });       

});