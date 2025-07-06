// ShipmentManager.js
import { getReadyToShipCoils } from '../../db/selectReadyForShipment.js';

export class ShipmentManager {
    constructor() {
        this.shipments = [];
        this.shipmentIdCounter = 0;
    }

    addShipment(shipment) {
        shipment.id = this.shipmentIdCounter++;
        this.shipments.push(shipment);
        return shipment.id;
    }

    getShipment(id) {
        return this.shipments.find(s => s.id === id);
    }

    getAllShipments() {
        return this.shipments;
    }

    removeShipment(id) {
        const index = this.shipments.findIndex(s => s.id === id);
        if (index !== -1) {
            this.shipments.splice(index, 1);
            return true;
        }
        return false;
    }

    getCoilsThatNeedShipment(){
        getReadyToShipCoils()
        .then(coils => {
            return coils.map(coil => ({
                material_id: coil.material_id,
                status: coil.status
            }));
        })
        .catch(err => {
            console.error('Error fetching coils ready for shipment:', err);
            return [];
        });
    }

    //TODO: Implement this method to return coils ready for shipment
    getCoilsForShipment(shipmentId) {
        const shipment = this.getShipment(shipmentId);
        if (!shipment) {
            throw new Error(`Shipment with ID ${shipmentId} not found`);
        }
        return shipment.coils || [];
    }
}