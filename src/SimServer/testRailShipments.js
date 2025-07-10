import { RailShipmentManager } from "./sim/components/RailshipmentManager.js";

(async () => {
  const railShipmentManager = new RailShipmentManager('ST21');

  await railShipmentManager.createAndRegisterRailcarsForShipments();
  await railShipmentManager.planLoadsForActiveRailcars();
  railShipmentManager.startPollingRailShipmentStatus();
})();
