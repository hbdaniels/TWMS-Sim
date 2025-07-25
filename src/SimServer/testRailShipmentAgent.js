import { RailShipmentAgent } from "./sim/components/RailshipmentAgent.js";

(async () => {
  const railShipmentManager = new RailShipmentAgent('ST21');
  await railShipmentManager.dispatchCoilsThatAreRailShipments();
  await railShipmentManager.createAndRegisterRailcarsForShipments();
  await railShipmentManager.planLoadsForActiveRailcars();
  railShipmentManager.startPollingRailShipmentStatus();
})();
