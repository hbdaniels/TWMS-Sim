import { TruckShipmentAgent } from "./sim/components/TruckShipmentAgent.js";

(async () => {
  const truckManager = new TruckShipmentAgent("ST21");

  await truckManager.createAndInsertTrucksForShipments();
  truckManager.startPollingTruckRegistration();
})();
