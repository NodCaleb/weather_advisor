import { ActivityType } from '../types/models';
import type { ActivityType as ActivityTypeValue } from '../types/models';

interface ActivitySelectorProps {
  isDisabled: boolean;
  selectedActivity: ActivityTypeValue | null;
  onSelect: (activity: ActivityTypeValue) => Promise<void>;
}

const ACTIVITY_OPTIONS: ActivityTypeValue[] = [
  ActivityType.Running,
  ActivityType.Cycling,
  ActivityType.Picnic,
  ActivityType.Walking,
];

export function ActivitySelector({ isDisabled, selectedActivity, onSelect }: ActivitySelectorProps) {
  return (
    <section className="activity-selector" aria-label="Select activity">
      <h2 className="activity-selector__title">Choose an activity</h2>
      <div className="activity-selector__options">
        {ACTIVITY_OPTIONS.map((activity) => (
          <button
            key={activity}
            type="button"
            className={`activity-selector__option${selectedActivity === activity ? ' is-active' : ''}`}
            disabled={isDisabled}
            onClick={() => void onSelect(activity)}
          >
            {activity}
          </button>
        ))}
      </div>
    </section>
  );
}
