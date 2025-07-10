import { TruckShipmentManager } from "./sim/components/TruckShipmentManager.js";

(async () => {
  const truckManager = new TruckShipmentManager("ST21");

  await truckManager.createAndInsertTrucksForShipments();
  truckManager.startPollingTruckRegistration();
})();
